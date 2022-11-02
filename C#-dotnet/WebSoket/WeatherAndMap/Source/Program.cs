using HtmlAgilityPack;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

#nullable disable warnings

namespace WeatherAndMap
{
    class Program
    {
        const string WEATHER_API_KEY = "기상청_API_KEY"; // 기상청 API KEY
        const string NAVER_API_KEY_ID = "네이버_API_Key_Id"; // 네이버 API Key Id
        const string NAVER_API_KEY = "네이버_API_Key"; // 네이버 API Key

        static void Main(string[] args)
        {
            /*
            // 호스트 아이피를 가져와서 서버 열기. / 클라우드 서버는 안 되는 경우가 있다.
            var hostIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
            var myServer = new MyServer(hostIp, 8888);
            */

            var myServer = new MyServer("서버_아이피", 8888);
            myServer.StartServer();

            Console.CancelKeyPress += delegate { myServer.StopServer(); };
            while (true)
            {
                myServer.ClientWiating();
                // myServer.ClientProcessing();
                // myServer.ClientStop();
            }
        }

        public class MyServer
        {
            private string _host;
            private int _port;
            private Socket? _server;

            public MyServer(string host, int port)
            {
                _host = host;
                _port = port;
            }

            public void StartServer()
            {
                _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _server.Bind(new IPEndPoint(IPAddress.Any, _port));
                _server.Listen(10);

                Console.WriteLine($"{_host}:{_port}로 웹서버 시작.");
            }

            public void StopServer()
            {
                Console.WriteLine("\n");

                if (_server != null)
                    _server?.Close();

                Console.WriteLine("서버 종료\n\n");
            }

            public void ClientWiating()
            {
                //Console.Write("\r클라이언트 대기중...                   ");
                _server?.BeginAccept(ClientProcessing, null);

                Thread.Sleep(100);
            }

            public void ClientProcessing(IAsyncResult ar)
            {
                Socket client = _server.EndAccept(ar);


                var addr = client?.RemoteEndPoint as IPEndPoint;
                string addrS = addr?.Address.ToString();

                // 위치를 가져오는 게 공인IP만 가능해서 공인IP 가져오기
                // 웹서버에서 돌리면 접속하는 주소가 공인IP가 된다.
                var Eaddr = new HttpClient().GetStringAsync("http://ipinfo.io/ip");
                string EaddrS = addrS;
                Console.WriteLine($"\n클라이언트 접속: {addrS}:{addr?.Port}, 접속시간: {DateTime.Now}, {EaddrS}");

                try
                {
                    Thread.Sleep(50);
                    var type = "";
                    var data = new Byte[8000];
                    client?.Receive(data);
                    var req = Encoding.UTF8.GetString(data);

                    var content = File.ReadAllText("/var/www/html/test/404.html");
                    byte[] img = null;

                    string status = "404 Not Found";

                    Console.WriteLine("클라이언트 요청 대기중 ...");

                    if (req.StartsWith("GET /hello HTTP/1.1"))
                    {
                        content = File.ReadAllText("/var/www/html/test/Hello.html");
                        status = "200 OK";
                    }
                    else if (req.StartsWith("GET /weather HTTP/1.1"))
                    {
                        type = "페이지";
                        // html 수정
                        SetWeatherHtml();

                        content = File.ReadAllText("/var/www/html/test/weather.html");

                        status = "200 OK";
                    }
                    else if (req.StartsWith("GET /mapimg HTTP/1.1"))
                    {
                        type = "지도";
                        WeatherAndMap weatmap = new WeatherAndMap(EaddrS);
                        weatmap.GetMap();
                        img = File.ReadAllBytes("/var/www/html/test/map.jpg");

                        status = "200 OK";
                        Console.WriteLine("\r지도 이미지 불러옴                                 ");
                    }
                    else if (req.StartsWith("GET /weatherinfo HTTP/1.1"))
                    {
                        type = "날씨";
                        // html의 iframe으로 만듦
                        // 공인IP를 날씨와지도에 넘겨주면서 생성
                        WeatherAndMap weatmap = new WeatherAndMap(EaddrS);
                        content = weatmap.GetWeather();

                        status = "200 OK";
                        Console.WriteLine("\r날씨 정보 불러옴                                   ");
                    }

                    if (img != null)
                    {
                        var imageLength = img.Length;
                        var res = $"HTTP/1.1 {status}\r\nContent-Length: {imageLength}\r\nContent-Type: image/jepg\r\n\r\n";
                        var resData = Encoding.UTF8.GetBytes(res);
                        client?.Send(resData);
                        client?.Send(img);
                    }
                    else
                    {
                        var contentLength = Encoding.UTF8.GetBytes(content).Length;
                        var res = $"HTTP/1.1 {status}\r\nContent-Length: {contentLength}\r\n\r\n{content}";
                        var resData = Encoding.UTF8.GetBytes(res);
                        client?.Send(resData);
                        Console.WriteLine("웹페이지 불러옴");
                    }

                    Console.WriteLine($"클라이언트 {type} 요청 처리: {status}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
                finally
                {
                    client.Close();
                    Console.WriteLine("클라이언트 종료");
                }
            }

            void SetWeatherHtml()
            {
                // html 파일을 불러와서
                var weatherHtml = File.ReadAllText("/var/www/html/test/weather.html");

                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(weatherHtml);

                // img와 ifrmae의 src를 현재 호스트 아이피의 링크로 바꾼다.
                html.DocumentNode.SelectSingleNode("//img[@id='map']").SetAttributeValue("src", $"http://{_host}:8888/mapimg");
                html.DocumentNode.SelectSingleNode("//iframe[@id='weather']").SetAttributeValue("src", $"http://{_host}:8888/weatherinfo");

                // 변경한 html을 저장한다.
                html.Save("weather.html");
            }
        }

        class WeatherAndMap
        {
            string? _posX = "";
            string? _posY = "";

            public WeatherAndMap(string addr)
            {
                // 생성자로 받은 아이피로 위치를 받아와 클래스의 속성값에 넣어준다.
                string url = "http://ip-api.com/json/" + addr;
                var location = new HttpClient().GetStringAsync(url).Result;
                var _locattion = location.Split(':', ',');
                _posX = _locattion[15];
                _posY = _locattion[17];
            }

            public void GetMap()
            {
                // http로 연결해서 지도를 바이트로 가져온다.
                Console.Write("\n지도 요청 중...");
                var url = "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster?w=500&h=300&center=" + _posY + "," + _posX + "&level=12";
                var map = new HttpClient();
                map.DefaultRequestHeaders.Add("X-NCP-APIGW-API-KEY-ID", NAVER_API_KEY_ID);
                map.DefaultRequestHeaders.Add("X-NCP-APIGW-API-KEY", NAVER_API_KEY);

                Console.Write("\r지도 이미지 가져오는 중...");
                byte[] responseContent = map.GetByteArrayAsync(url).Result;

                // 바이트를 이미지 형태로 저장한다.
                System.IO.File.WriteAllBytes(@"/var/www/html/test/map.jpg", responseContent);
            }


            public string GetWeather()
            {
                Console.Write("\n날씨 요청 중...");

                // 기상청의 초단기 실황 가져오기
                var Url = $"http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getUltraSrtNcst?serviceKey={WEATHER_API_KEY}&numOfRows=10&dataType=JSON&pageNo=1";

                // 요구하는 정보로 URL을 만들어 날씨 정보를 가져온다.
                // 발표시각 오늘 날짜 / 현재 시각에서 1시간 빼기 / 해결
                DateTime Today = DateTime.Now.AddHours(-1);
                Url += "&base_date=" + Today.ToString("yyyyMMdd");
                Url += "&base_time=" + Today.ToString("hh") + "00";
                // 위치는 계산이 필요하다. / 해결  / 밑에 계산하는 부분을 가져왔다.
                Dictionary<string, object> pos = getGridxy(double.Parse(_posX), double.Parse(_posY));
                var posX = pos["x"].ToString();
                var posY = pos["y"].ToString();
                Url += "&nx=" + posX;
                Url += "&ny=" + posY;

                Console.Write("\r날씨 정보 가져오는 중...");
                var result = new HttpClient().GetStringAsync(Url).Result.ToString();

                return MakeWeather(ParsingWeather(result));
            }
            delegate string rain();

            string MakeWeather(Dictionary<string, string> weather)
            {

                // 강수형태에 맞는 정보로 바꾸기
                string rainType = weather["강수형태"];
                switch (rainType)
                {
                    case "0": rainType = "맑음"; break;
                    case "1": rainType = "비"; break;
                    case "2": rainType = "비/눈"; break;
                    case "3": rainType = "눈"; break;
                    case "5": rainType = "빗방울"; break;
                    case "6": rainType = "빗방울눈날림"; break;
                    case "7": rainType = "눈날림"; break;
                }

                // 풍향의 맞는 정보로 바꾸기
                string windDir = Math.Round(((int.Parse(weather["풍향"]) + (22.5 * 0.5f)) / 22.5f)).ToString();
                switch (windDir)
                {
                    case "0": windDir = "북"; break;
                    case "1": windDir = "북북동"; break;
                    case "2": windDir = "북동"; break;
                    case "3": windDir = "동북동"; break;
                    case "5": windDir = "동"; break;
                    case "6": windDir = "동남동"; break;
                    case "7": windDir = "남동"; break;
                    case "8": windDir = "남남동"; break;
                    case "9": windDir = "남"; break;
                    case "10": windDir = "남남서"; break;
                    case "11": windDir = "남서"; break;
                    case "12": windDir = "서남서"; break;
                    case "13": windDir = "서"; break;
                    case "14": windDir = "서북서"; break;
                    case "15": windDir = "북서"; break;
                    case "16": windDir = "북북서"; break;
                }


                string html = "<!DOCTYPE html><html lang=\"kr\"><head>";
                html += "<meta charset=\"utf-8\">";
                html += "<style>tr,td {text-align: autor;font-size:1.1em;width: 150px;}</style></head><body>";
                html += "<table>";
                html += $"<tr><td>기온</td><td>{weather["기온"]} ℃</td></tr>";
                html += $"<tr><td>습도</td><td>{weather["습도"]} %</td></tr>";
                html += $"<tr><td>강수형태</td><td>{rainType}</td></tr>";
                html += $"<tr><td>1시간 강수량</td><td>{weather["1시간 강수량"]} mm</td></tr>";
                html += $"<tr><td>풍향</td><td>{windDir}</td></tr>";
                html += $"<tr><td>풍속</td><td>{weather["풍속"]} m/s</td></tr>";
                html += $"<tr><td>동서바람성분</td><td>{weather["동서바람성분"]} m/s</td></tr>";
                html += $"<tr><td>남북바람성분</td><td>{weather["남북바람성분"]} m/s</td></tr>";
                html += "</table>";
                html += "</body></html>";

                return html;
            }

            Dictionary<string, string> ParsingWeather(string weatherInfo)
            {
                /*
                // Newtonsoft.Json.Linq 사용했을 때

                // 받은 문자열을 Json으로 변환하고 토큰으로 만든다.
                JObject? result = JObject.Parse(weatherInfo);
                JToken? resultT = result["response"]?["body"]?["items"]?["item"];

                // 토큰을 리스트 안에 차례대로 넣는다. 
                var weather = new List<string[]>();
                foreach(JToken t in resultT)
                    weather.Add(new string[]{ t["category"].Value<string>(), t["obsrValue"].Value<string>() });
                */

                // System.Text.Json 사용했을 때   /   길어진다.
                JsonDocument result = JsonDocument.Parse(weatherInfo);
                JsonElement resultE = result.RootElement.GetProperty("response").GetProperty("body").GetProperty("items").GetProperty("item");

                var weather = new List<string[]>();
                foreach (JsonElement t in resultE.EnumerateArray())
                    weather.Add(new string[] { t.GetProperty("category").ToString(), t.GetProperty("obsrValue").ToString() });


                // 날씨 카테고리를 내용으로 바꾼다.
                Dictionary<string, string> category = new Dictionary<string, string>();
                category.Add("T1H", "기온");
                category.Add("RN1", "1시간 강수량");
                category.Add("UUU", "동서바람성분");
                category.Add("VVV", "남북바람성분");
                category.Add("REH", "습도");
                category.Add("PTY", "강수형태");
                category.Add("VEC", "풍향");
                category.Add("WSD", "풍속");

                // 날씨와지도 클래스의 속성 값에 넣어준다.
                Dictionary<string, string> _weather = new Dictionary<string, string>();
                foreach (string[] w in weather)
                    _weather.Add(category[w[0]], w[1]);

                return _weather;
            }



            // 경도와 위도를 좌표값으로 바꿔주는 구문
            // 인터넷에서 가져와서 C#에 맞게 수정했습니다.
            // 해당 링크 : https://m.blog.naver.com/PostView.naver?isHttpsRedirect=true&blogId=tkddlf4209&logNo=220632424141
            public Dictionary<string, object> getGridxy(double v1, double v2)
            {
                double RE = 6371.00877; // 지구 반경(km)
                double GRID = 5.0; // 격자 간격(km)
                double SLAT1 = 30.0; // 투영 위도1(degree)
                double SLAT2 = 60.0; // 투영 위도2(degree)
                double OLON = 126.0; // 기준점 경도(degree)
                double OLAT = 38.0; // 기준점 위도(degree)
                double XO = 43; // 기준점 X좌표(GRID)
                double YO = 136; // 기1준점 Y좌표(GRID)

                double DEGRAD = Math.PI / 180.0;
                // double RADDEG = 180.0 / Math.PI;

                double re = RE / GRID;
                double slat1 = SLAT1 * DEGRAD;
                double slat2 = SLAT2 * DEGRAD;
                double olon = OLON * DEGRAD;
                double olat = OLAT * DEGRAD;

                double sn = Math.Tan(Math.PI * 0.25 + slat2 * 0.5) / Math.Tan(Math.PI * 0.25 + slat1 * 0.5);
                sn = Math.Log(Math.Cos(slat1) / Math.Cos(slat2)) / Math.Log(sn);
                double sf = Math.Tan(Math.PI * 0.25 + slat1 * 0.5);
                sf = Math.Pow(sf, sn) * Math.Cos(slat1) / sn;
                double ro = Math.Tan(Math.PI * 0.25 + olat * 0.5);
                ro = re * sf / Math.Pow(ro, sn);
                Dictionary<string, object> map = new Dictionary<string, object>();
                map.Add("lat", v1);
                map.Add("lng", v2);
                double ra = Math.Tan(Math.PI * 0.25 + (v1) * DEGRAD * 0.5);
                ra = re * sf / Math.Pow(ra, sn);
                double theta = v2 * DEGRAD - olon;
                if (theta > Math.PI)
                    theta -= 2.0 * Math.PI;
                if (theta < -Math.PI)
                    theta += 2.0 * Math.PI;
                theta *= sn;

                map.Add("x", Math.Floor(ra * Math.Sin(theta) + XO + 0.5));
                map.Add("y", Math.Floor(ro - ra * Math.Cos(theta) + YO + 0.5));

                return map;
            }
        }
    }
}

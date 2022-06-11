import org.json.simple.JSONObject;
import org.json.simple.parser.JSONParser;
import org.json.simple.parser.ParseException;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.HashMap;
import java.util.Map;

public class KakaoPay {

    String AdminKey = "카카오_어드민_키"; // 카카오 어드민 키
    String StoreKey = "TC0ONETIME"; // 상점 아이디 ( 테스트키 : TC0ONETIME )
    String ServiceUrl = "kapi.kakao.com"; // 서비스 URL
    // String MonthStoreKey = ""; // 상점 아이디 ( 정기결제 )
    // String TestMonthStoreKey = "TCSUBSCRIP"; // 상점 아이디 ( 정기결제 테스트키 : TCSUBSCRIP )
    HashMap<String, String> PayPostUrl = new HashMap<String, String>();

    String ApprovalUrl = "http://localhost:8080/kakaoPaySuccess"; // 결제 성공 URL
    String FailUrl = "http://localhost:8080/kakaoPaySuccessFail"; // 결제 실패 URL
    String CancelUrl = "http://localhost:8080/kakaoPayCancel"; // 결제 취소 URL

    String partner_order_id;
    String partner_user_id;
    String item_name;
    String quantity;
    String total_amount;
    String tax_free_amount;
    String vat_amount;
    String cancel_amount;

    String tid;
    String pg_token;

    KakaoPay(Map<String, String> Info) {
        PayPostUrl.put("ready", "https://kapi.kakao.com/v1/payment/ready"); // 결제 준비 요청
        PayPostUrl.put("approve", "https://kapi.kakao.com/v1/payment/approve"); // 결제 승인 요청
        PayPostUrl.put("order", "https://kapi.kakao.com/v1/payment/order");
        PayPostUrl.put("cancel", "https://kapi.kakao.com/v1/payment/cancel");

        partner_order_id = Info.get("OrderNo");
        partner_user_id = Info.get("UserNo");
        item_name = Info.get("ProdName");
        quantity = Info.get("quantity");
        total_amount = Info.get("TotalPrice");
        tax_free_amount = Info.get("tax_free_amount");
        vat_amount = Info.get("Vat");

    }
    /*
     * public void setStoreKey(String Type)
     * {
     * if( Type != ' ' )
     * {
     * Type = Type.toUpperCase();
     * if( Type == "S" ) this.StoreKey = this.StoreKey;
     * else if( Type == "T" ) StoreKey = MonthStoreKey;
     * else if( Type == "TM" ) StoreKey = TestMonthStoreKey;
     * else StoreKey = TestStoreKey;
     * }
     * }
     */

    void acc(String type) {

        if (type.contains("!")) {
            String[] e = type.split("!");
            type = e[0];

            switch (e[0]) {
                case "approve" -> pg_token = e[1];
                case "cancel" -> cancel_amount = e[1];
            }
        }
        try {
            URL t = new URL(PayPostUrl.get(type));
            HttpURLConnection test = (HttpURLConnection) t.openConnection();
            test.setRequestMethod("POST");
            test.setRequestProperty("Host", ServiceUrl);
            test.setRequestProperty("Authorization", "KakaoAK " + AdminKey);
            test.setRequestProperty("Content-type", "application/x-www-form-urlencoded;charset=utf-8");
            test.setDoInput(true);
            test.setDoOutput(true);

            switch (type) {
                case "ready" -> test.getOutputStream().write(Ready().getBytes());
                case "approve" -> test.getOutputStream().write(Approve().getBytes());
                case "order" -> test.getOutputStream().write(Order().getBytes());
                case "cancel" -> test.getOutputStream().write(Cancel().getBytes());
            }

            System.out.println(t + " " + test);

            BufferedReader in = new BufferedReader(new InputStreamReader(test.getInputStream()));

            JSONParser parser = new JSONParser();
            JSONObject object = (JSONObject) parser.parse(in);

            PayPrint payPrint = new PayPrint(object);
            switch (type) {
                case "ready" -> {
                    payPrint.Ready();
                    this.tid = (String) object.get("tid");
                }
                case "approve" -> payPrint.Approve();
                case "order" -> payPrint.Order();
                case "cancel" -> payPrint.Cancel();
            }

        } catch (IOException | ParseException e) {
            e.printStackTrace();
        }
    }

    public String Ready() {

        Map<String, String> params = new HashMap<>();

        params.put("cid", StoreKey);
        params.put("partner_order_id", partner_order_id);
        params.put("partner_user_id", partner_user_id);
        params.put("item_name", item_name);
        params.put("quantity", quantity);
        params.put("total_amount", total_amount);
        params.put("tax_free_amount", tax_free_amount);
        // params.put("vat_amount" , vat_amount);
        params.put("approval_url", ApprovalUrl);
        params.put("cancel_url", CancelUrl);
        params.put("fail_url", FailUrl);

        String stringParams = "";
        for (Map.Entry<String, String> elem : params.entrySet()) {
            stringParams += (elem.getKey() + "=" + elem.getValue() + "&");
        }
        System.out.println(stringParams);
        return stringParams;
    }

    public String Approve() {
        Map<String, String> params = new HashMap<String, String>();

        /*
         * cid String 가맹점 코드, 10자 O
         * tid String 결제 고유번호, 결제 준비 API 응답에 포함 O
         * partner_order_id String 가맹점 주문번호, 결제 준비 API 요청과 일치해야 함 O
         * partner_user_id String 가맹점 회원 id, 결제 준비 API 요청과 일치해야 함 O
         * pg_token String 결제승인 요청을 인증하는 토큰
         * 사용자 결제 수단 선택 완료 시, approval_url로 redirection해줄 때 pg_token을 query string으로 전달
         * O
         * payload String 결제 승인 요청에 대해 저장하고 싶은 값, 최대 200자 X
         * total_amount Integer 상품 총액, 결제 준비 API 요청과 일치해야 함
         */
        params.put("cid", StoreKey);
        params.put("tid", tid);
        params.put("partner_order_id", partner_order_id);
        params.put("partner_user_id", partner_user_id);
        params.put("pg_token", pg_token);

        String stringParams = "";
        for (Map.Entry<String, String> elem : params.entrySet()) {
            stringParams += (elem.getKey() + "=" + elem.getValue() + "&");
        }
        return stringParams;
    }

    public String Order() {

        Map<String, String> params = new HashMap<>();
        /*
         * cid String 가맹점 코드, 10자 O
         * cid_secret String 가맹점 코드 인증키, 24자, 숫자+영문 소문자 조합 X
         * tid String 결제 고유번호, 20자 O
         */
        params.put("cid", StoreKey);
        params.put("tid", tid);

        String stringParams = "";
        for (Map.Entry<String, String> elem : params.entrySet()) {
            stringParams += (elem.getKey() + "=" + elem.getValue() + "&");
        }
        return stringParams;
    }

    public String Cancel() {

        Map<String, String> params = new HashMap<>();
        /*
         * cid String 가맹점 코드, 10자 O
         * cid_secret String 가맹점 코드 인증키, 24자, 숫자+영문 소문자 조합 X
         * tid String 결제 고유번호 O
         * cancel_amount Integer 취소 금액 O
         * cancel_tax_free_amount Integer 취소 비과세 금액 O
         * cancel_vat_amount Integer 취소 부가세 금액
         * 요청 시 값을 전달하지 않을 경우, (취소 금액 - 취소 비과세 금액)/11, 소숫점이하 반올림 X
         * cancel_available_amount Integer 취소 가능 금액(결제 취소 요청 금액 포함) X
         * payload String 해당 요청에 대해 저장하고 싶은 값, 최대 200자 X
         */
        params.put("cid", StoreKey);
        params.put("tid", tid);
        params.put("cancel_amount", cancel_amount);
        params.put("cancel_tax_free_amount", "0");
        params.put("cancel_available_amount", "");

        String stringParams = "";
        for (Map.Entry<String, String> elem : params.entrySet()) {
            stringParams += (elem.getKey() + "=" + elem.getValue() + "&");
        }
        return stringParams;
    }

}
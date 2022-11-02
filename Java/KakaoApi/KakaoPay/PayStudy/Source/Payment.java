import java.util.HashMap;
import java.util.Map;
import java.util.Scanner;

public class Payment {

    public static void main(String[] args) {
        Scanner input = new Scanner(System.in);

        Map<String, String> Info = new HashMap<>();
        Info.put("OrderNo", "122523");     // 가맹점 주문번호. 최대 100자	O	String
        Info.put("UserNo", "test011");   // 가맹점 회원 id. 최대 100자	O	String
        Info.put("ProdName", "테스트");     // 상품명. 최대 100자	O	String
        Info.put("quantity", "1");         // 상품 수량	O	Integer
        Info.put("TotalPrice", "5000");        // 상품 총액	O	Integer
        Info.put("tax_free_amount", "0");         // 상품 비과세 금액	O	Integer
        Info.put("Vat", "");          // 상품 부가세 금액(안보낼 경우 (상품총액 - 상품 비과세 금액)/11 : 소숫점이하 반올림)	X	Integer


        KakaoPay kakaoPay = new KakaoPay(Info);

        int num;
        do{
            System.out.println("1: 준비 / 2: 승인 / 3: 내역 / 4: 취소 / 0: 종료 ");
            num = input.nextInt();
            String type = "";
            switch (num){
                case 1 -> type = "ready";
                case 2 -> type = "approve!" + input.next();
                case 3 -> type = "order";
                case 4 -> type = "cancel!" + input.nextInt();
                case 0 -> System.out.println("종료");
                default -> System.out.println("잘못된 입력");
            }
            if(1 <= num && num <= 4) kakaoPay.acc(type);

        } while (num != 0);

    }
}

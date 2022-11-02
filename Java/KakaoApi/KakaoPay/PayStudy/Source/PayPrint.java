import org.json.simple.JSONObject;

public class PayPrint {
    JSONObject object;

    PayPrint(JSONObject object){
        this.object = object;
    }

    void Ready(){
        String successURL = (String) object.get("next_redirect_pc_url");
        System.out.println(successURL);
    }

    void Approve(){
        System.out.print("이름 : ");
        System.out.println(object.get("item_name"));

        System.out.print("가맹점 코드 : ");
        System.out.println(object.get("partner_order_id"));

        System.out.print("금액 : ");
        System.out.println(object.get("amount"));

        if(object.get("card_info") != null) {
            System.out.print("카드 정보 : ");
            System.out.println(object.get("card_info"));
        }

        System.out.print("결제 수단 : ");
        System.out.println(object.get("payment_method_type"));

/*
            aid	String	요청 고유 번호
            tid	String	결제 고유 번호
            cid	String	가맹점 코드
            sid	String	정기결제용 ID, 정기결제 CID로 단건결제 요청 시 발급
            partner_order_id	String	가맹점 주문번호, 최대 100자
            partner_user_id	String	가맹점 회원 id, 최대 100자
            payment_method_type	String	결제 수단, CARD 또는 MONEY 중 하나
            amount	Amount	결제 금액 정보
            card_info	CardInfo	결제 상세 정보, 결제수단이 카드일 경우만 포함
            item_name	String	상품 이름, 최대 100자
            item_code	String	상품 코드, 최대 100자
            quantity	Integer	상품 수량
            created_at	Datetime	결제 준비 요청 시각
            approved_at	Datetime	결제 승인 시각
            payload	String	결제 승인 요청에 대해 저장한 값, 요청 시 전달된 내용

 */
    }

    void Order(){
        System.out.print("이름 : ");
        System.out.println(object.get("item_name"));

        System.out.print("가맹점 코드 : ");
        System.out.println(object.get("partner_order_id"));

        System.out.print("금액 : ");
        System.out.println(object.get("amount"));

        System.out.print("결제 수단 : ");
        System.out.println(object.get("payment_method_type"));

/*
            aid	String	요청 고유 번호
            tid	String	결제 고유 번호
            cid	String	가맹점 코드
            sid	String	정기결제용 ID, 정기결제 CID로 단건결제 요청 시 발급
            partner_order_id	String	가맹점 주문번호, 최대 100자
            partner_user_id	String	가맹점 회원 id, 최대 100자
            payment_method_type	String	결제 수단, CARD 또는 MONEY 중 하나
            amount	Amount	결제 금액 정보
            card_info	CardInfo	결제 상세 정보, 결제수단이 카드일 경우만 포함
            item_name	String	상품 이름, 최대 100자
            item_code	String	상품 코드, 최대 100자
            quantity	Integer	상품 수량
            created_at	Datetime	결제 준비 요청 시각
            approved_at	Datetime	결제 승인 시각
            payload	String	결제 승인 요청에 대해 저장한 값, 요청 시 전달된 내용

 */
    }

    void Cancel(){
        System.out.print("이름 : ");
        System.out.println(object.get("item_name"));

        System.out.print("가맹점 코드 : ");
        System.out.println(object.get("partner_order_id"));

        System.out.print("금액 : ");
        System.out.println(object.get("amount"));

        System.out.print("카드 정보 : ");
        System.out.println(object.get("card_info"));

        System.out.print("결제 수단 : ");
        System.out.println(object.get("payment_method_type"));

/*
            aid	String	요청 고유 번호
            tid	String	결제 고유 번호
            cid	String	가맹점 코드
            sid	String	정기결제용 ID, 정기결제 CID로 단건결제 요청 시 발급
            partner_order_id	String	가맹점 주문번호, 최대 100자
            partner_user_id	String	가맹점 회원 id, 최대 100자
            payment_method_type	String	결제 수단, CARD 또는 MONEY 중 하나
            amount	Amount	결제 금액 정보
            card_info	CardInfo	결제 상세 정보, 결제수단이 카드일 경우만 포함
            item_name	String	상품 이름, 최대 100자
            item_code	String	상품 코드, 최대 100자
            quantity	Integer	상품 수량
            created_at	Datetime	결제 준비 요청 시각
            approved_at	Datetime	결제 승인 시각
            payload	String	결제 승인 요청에 대해 저장한 값, 요청 시 전달된 내용

 */
    }
}

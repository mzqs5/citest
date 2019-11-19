using IF.Payment.Concrete.AliPay.Model;

namespace IF.Payment.SDK
{
    public class PayDemo
    {
        public static string PayDo()
        {
            AliQuery payparam = new AliQuery();
            var rst = PaymentManager.PaymentRequest(payparam);
            if (rst.Status)//唤起支付
                PaymentManager.Success(rst.Content, null);

            return rst.ErrorMsg;
        }
    }
}

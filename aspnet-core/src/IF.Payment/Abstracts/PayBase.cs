using IF.Payment.Model;

namespace IF.Payment.Abstracts
{
    public abstract class PayBase
    {
        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected abstract PayRst Pay(QueryBase query);


        public PayRst Execute(QueryBase query)
        {
            return Pay(query);
        }



    }



}

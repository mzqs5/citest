namespace IF.Payment.Model
{
    public class PayRst
    {
        /// <summary>
        /// true 成功 false 失败
        /// </summary>
       public bool Status { get; set; }
        /// <summary>
        /// 失败信息
        /// </summary>
       public string ErrorMsg { get; set; }

       public string Content { get; set; }
    }
}

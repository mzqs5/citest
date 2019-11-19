using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DbHelper
{
    /// <summary>
    /// Sql操作助手
    /// </summary>
    public interface ISqlHelper
    {
        /// <summary>
        /// 当前连接字符串
        /// </summary>
        IDbConnection Connection { get; set; }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="parms">参数</param>
        /// <returns>返回泛型的实体对象</returns>
        IEnumerable<T> Query<T>(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms) where T : class;

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="func">返回IDbDataParameter[]的委托</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回泛型的实体对象</returns>
        IEnumerable<T> Query<T>(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text) where T : class;

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="parms">参数</param>
        /// <returns>首行首列的值</returns>
        object Scalar(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms);

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="func">返回IDbDataParameter[]的委托</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>首行首列的值</returns>
        object Scalar(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text);

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <typeparam name="T">仅支持基础类型</typeparam>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        T Scalar<T>(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms) where T : struct;

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <typeparam name="T">仅支持基础类型</typeparam>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="func">返回IDbDataParameter[]的委托</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>首行首列的值</returns>
        T Scalar<T>(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text) where T : struct;


        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="parms">参数</param>
        /// <returns>返回影响行数</returns>
        int Execute(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms);


        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="func">返回IDbDataParameter[]的委托</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        int Execute(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text);
    }
}

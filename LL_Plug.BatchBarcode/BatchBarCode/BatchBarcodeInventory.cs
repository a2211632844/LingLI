using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LL_Plug.BatchBarcode.BatchBarCode
{
    class BatchBarcodeInventory
    {
        /// <summary>
        /// 条码打印
        /// </summary>
        /// <param name="jsonArrayText"></param>
        /// <param name="user"></param>
        /// <param name="psw"></param>
        /// <param name="dbid"></param>
        /// <param name="apiurl"></param>
        /// <returns></returns>
        public static string JsonAdd(string jsonArrayText, string user, string psw, string dbid, string apiurl)
        {

            string strresult = string.Empty;
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonArrayText);
            //金蝶云组件
            K3CloudApiClient client = new K3CloudApiClient(apiurl);
            var loginResult = client.Login(
                   dbid,
                   user,
                   psw,
                   2052);

            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";

            if (loginResult == true)
            {
                JObject jsonRoot = new JObject();
                jsonRoot.Add("IsAutoSubmitAndAudit", true);

                // Model: 单据详细数据参数
                JObject model = new JObject();

                jsonRoot.Add("Model", model);
                // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
                model.Add("FID", 0);

                // 普通字段

                // 基础资料，填写编码
                model.Add("FBillNo", jo["FBillNo"]);//条码
                model.Add("F_ora_BarcodeDate", jo["F_ora_BarcodeDate"]);//日期
                model.Add("F_ora_ProductinOrdNum", jo["F_ora_ProductinOrdNum"]);//生产订单号
                model.Add("F_ora_ProductNum", new JObject() { { "FNumber", jo["F_ora_ProductNum"] } });//物料编码
                model.Add("F_ora_Unit", new JObject() { { "FNumber", jo["F_ora_Unit"] } });//单位
                model.Add("F_ora_ProductOrdQty", jo["F_ora_ProductOrdQty"]);//生产订单数量
                model.Add("F_ora_PercaseQty", jo["F_ora_PercaseQty"]);//每箱数量
                model.Add("F_ora_InstockBillNo", jo["F_ora_InstockBillNo"]);//生产入库单单号
                model.Add("F_ora_BillNo", jo["F_ora_BillNo"]);//序号
                model.Add("F_ORA_PRINTQTY", jo["F_ORA_PRINTQTY"]);
                model.Add("F_ora_ProductDate", jo["F_ora_ProductDate"]);
                model.Add("F_ora_SumBoxQty", jo["F_ora_SumBoxQty"]);
                model.Add("F_ora_BoxNo", jo["F_ora_BoxNo"]);
                model.Add("F_LLL_BOMBH", jo["F_LLL_BOMBH"]);
                model.Add("F_LLL_BOMMC", jo["F_LLL_BOMMC"]  );//bomid
                model.Add("F_LLL_KHPH",jo["F_LLL_KHPH"]);//客户批号
                model.Add("F_LLL_TGFS",jo["F_LLL_TGFS"]);//套管方式
                model.Add("F_LLL_HTH",jo["F_LLL_HTH"]);//合同号
                model.Add("F_LLL_KHMaterialID", jo["F_LLL_KHMaterialID"] );//客户物料代码
                model.Add("F_LLL_KHMaterialName",jo["F_LLL_KHMaterialName"]);//客户物料名称
                model.Add("F_LLL_SCXText", jo["FSCX"] );
                model.Add("F_LLL_BZTest", jo["FBZ"] );
                model.Add("F_LLL_KH",jo["F_LLL_KH"]);

                // 调用Web API接口服务，保存即时库存汇总
                result = client.Execute<string>(
                    "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save",
                    new object[] { "k88c57702012242e6b81671fe123aa125", jsonRoot.ToString() });
            }
            return result;
        }
    }
}

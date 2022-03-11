using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using LL_Plug.BatchBarcode.BatchBarCode;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.WebApi.Client;

namespace LL_Plug.BatchBarcode
{
    [HotUpdate]
    public class ImportBarcodeDynamicFormPlugIn : AbstractDynamicFormPlugIn
    {
        public static int LSH;

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string SCDDDATE = Convert.ToString(this.View.OpenParameter.GetCustomParameter("scddDate"));//生产订单日期    
            string FID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("Fid"));//单据编号
            string UNIT = Convert.ToString(this.View.OpenParameter.GetCustomParameter("unit"));//单位
            string FMATERIALID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FMaterialId"));//物料num
            string FMATERIALIDNAME = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FMATERIALID"));//物料name
            string FMATERIALIDNUMBER = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FMATERIALIDNumber"));//物料name
            string GGXH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("GGXH"));//规格型号
            string QTY = Convert.ToString(this.View.OpenParameter.GetCustomParameter("qty"));//数量
            string UNITNAME = Convert.ToString(this.View.OpenParameter.GetCustomParameter("UNITNAME"));//数量
            string FDeliveryDate = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FDeliveryDate"));//要货日期
            string FOrderNo = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FOrderNo"));//销售订单号
            string FCust = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FCust"));//客户
            string FOrderQty = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FOrderQty"));//销售订单数量
            string BOMID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("BOMID"));//BomID
            string F_LLL_KHPH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_KHPH"));//F_LLL_KHPH客户批号
            string TGFS = Convert.ToString(this.View.OpenParameter.GetCustomParameter("TGFS"));//TGFS套管方式
            string F_LLL_HTH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_HTH"));//合同号
            string F_LLL_KHMaterialID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_KHMaterialID"));//客户物料编码
            string F_LLL_KHMaterialName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_KHMaterialName"));//客户物料名称
            string F_LLL_KH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_KH"));//客户
            string newTGFS = "";
            if (TGFS.IsNullOrEmptyOrWhiteSpace()==false) 
            {
                string sqlTGFS = string.Format("select FDATAVALUE from T_BAS_ASSISTANTDATAENTRY_L where FENTRYID = '{0}'",TGFS); //F_LLL_TGFS
                DataSet dsTGFS = DBServiceHelper.ExecuteDataSet(this.Context, sqlTGFS);
                DataTable dtTGFS = dsTGFS.Tables[0];
                newTGFS = dtTGFS.Rows[0][0].ToString();
            }

            //把值赋到文本框内
            this.Model.SetValue("F_ora_Date", SCDDDATE);
            this.Model.SetValue("F_ora_Material", FMATERIALID);
            this.Model.SetValue("F_ora_MaterialName", FMATERIALIDNAME);
            this.Model.SetValue("F_ora_Spec", GGXH);
            this.Model.SetValue("F_ora_MoBillNo", FID);
            this.Model.SetValue("F_ora_MoQty", QTY);
            this.Model.SetValue("F_ora_Unit", UNIT);
            this.Model.SetValue("FDeliveryDate", FDeliveryDate);
            this.Model.SetValue("FOrderNo", FOrderNo);
            this.Model.SetValue("FCust", FCust);
            this.Model.SetValue("FOrderQty", FOrderQty);
            this.Model.SetValue("F_ora_MoSeq", this.View.OpenParameter.GetCustomParameter("FSeq"));
            this.Model.SetValue("F_LLL_BOM", BOMID);
            this.Model.SetValue("F_LLL_KHPH", F_LLL_KHPH);
            this.Model.SetValue("F_LLL_TGFS", newTGFS);



            string sql = string.Format(@"/*dialect*/
                                        select F_ORA_PRODUCTINORDNUM 
                                        ,F_ORA_PRODUCTNUM 
                                        ,FBillNo  
                                        ,FID
                                        ,F_ora_PercaseQty  
                                        ,F_ORA_PRINTTIMES
                                        ,sum(F_ora_PercaseQty) as Fbarcodesum
                                        from ora_t_Cust100006 
                                        where F_ORA_PRODUCTINORDNUM = '{0}'and F_ORA_PRODUCTNUM ='{1}' group by  F_ORA_PRODUCTINORDNUM 
                                        ,F_ORA_PRODUCTNUM ,FBillNo  ,F_ora_PercaseQty, FID,F_ORA_PRINTTIMES ", FID, FMATERIALID);
            DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);
            DataTable dt = ds.Tables[0];

            Entity entity = this.View.BillBusinessInfo.GetEntity("F_ora_Entity1");
            //当条码存在时 给历史条码添加信息
            if (dt.Rows.Count > 0)
            {
                decimal BarcodeSum = Convert.ToInt32(dt.Rows[0]["Fbarcodesum"]);
                this.Model.SetValue("F_ora_BarcodeSumQty", BarcodeSum);//条码汇总数量
                DynamicObjectCollection rows = this.Model.GetEntityDataObject(entity);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //每一行的值定义成 DynamicObject类型
                    DynamicObject row = new DynamicObject(entity.DynamicObjectType);
                    //entity.SeqDynamicProperty.SetValue(row, i -1);
                    //第i行
                    row["F_ora_ProductionOrdNumE"] = dt.Rows[i]["F_ora_ProductinOrdNum"].ToString();
                    row["F_ora_MaterialidE"] = dt.Rows[i]["F_ora_ProductNum"].ToString();
                    row["F_ora_BarcodeEntry"] = dt.Rows[i]["FBillNo"].ToString();
                    //row["F_ora_EntryBoxQtyEntry1"] = Math.Round(Convert.ToDouble(dt.Rows[i]["F_ora_PercaseQty"].ToString()));
                    decimal de = Convert.ToDecimal(dt.Rows[i]["F_ora_PercaseQty"]);
                    row["F_ora_EntryBoxQtyEntry1"] = Convert.ToDecimal(dt.Rows[i]["F_ora_PercaseQty"]);
                    row["F_ora_HBarcodeId"] = Convert.ToInt32(dt.Rows[i]["FID"].ToString());
                    row["F_ora_PrintedQty"] = Convert.ToInt32(dt.Rows[i]["F_ORA_PRINTTIMES"]);
                    //Convert.ToInt32(dt.Rows[i]["F_ora_PercaseQty"].ToString());
                    this.View.Model.CreateNewEntryRow(entity, -1);
                    this.Model.ClearNoDataRow();
                    rows.Add(row);
                }
                this.View.UpdateView("F_ora_Entity1");
            }
            //当该条码不存在时
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key.ToString() == "F_ora_MoBillNo")
            {
                string mosql = string.Format(@"/*dialect*/exec w_sp_getmoinfo '{0}'",e.NewValue.ToString());

                DataSet mods = DBServiceHelper.ExecuteDataSet(this.Context, mosql);
                DataTable modt = mods.Tables[0];

                if (modt.Rows.Count > 0)
                {
                    string SCDDDATE = Convert.ToString(modt.Rows[0]["scddDate"]);//生产订单日期    
                    string FID = Convert.ToString(modt.Rows[0]["Fid"]);//单据编号
                    string UNIT = Convert.ToString(modt.Rows[0]["unit"]);//单位
                    string FMATERIALID = Convert.ToString(modt.Rows[0]["FMaterialId"]);//物料num
                    string FMATERIALIDNAME = Convert.ToString(modt.Rows[0]["FMATERIALID"]);//物料name
                    string FMATERIALIDNUMBER = Convert.ToString(modt.Rows[0]["FMATERIALIDNumber"]);//物料name
                    string GGXH = Convert.ToString(modt.Rows[0]["GGXH"]);//规格型号
                    string QTY = Convert.ToString(modt.Rows[0]["qty"]);//数量
                    string UNITNAME = Convert.ToString(modt.Rows[0]["UNITNAME"]);//数量
                    string FDeliveryDate = Convert.ToString(modt.Rows[0]["FDeliveryDate"]);//要货日期
                    string FOrderNo = Convert.ToString(modt.Rows[0]["FOrderNo"]);//销售订单号
                    string FCust = Convert.ToString(modt.Rows[0]["FCust"]);//客户
                    string FOrderQty = Convert.ToString(modt.Rows[0]["FOrderQty"]);//销售订单数量

                    //把值赋到文本框内
                    this.Model.SetValue("F_ora_Date", SCDDDATE);
                    this.Model.SetValue("F_ora_Material", FMATERIALID);
                    this.Model.SetValue("F_ora_MaterialName", FMATERIALIDNAME);
                    this.Model.SetValue("F_ora_Spec", GGXH);
                    this.Model.SetValue("F_ora_MoBillNo", FID);
                    this.Model.SetValue("F_ora_MoQty", QTY);
                    this.Model.SetValue("F_ora_Unit", UNIT);
                    this.Model.SetValue("FDeliveryDate", FDeliveryDate);
                    this.Model.SetValue("FOrderNo", FOrderNo);
                    this.Model.SetValue("FCust", FCust);
                    this.Model.SetValue("FOrderQty", FOrderQty);
                    this.Model.SetValue("F_ora_MoSeq", this.View.OpenParameter.GetCustomParameter("FSeq"));


                    string sql = string.Format(@"/*dialect*/
                                        select F_ORA_PRODUCTINORDNUM 
                                        ,F_ORA_PRODUCTNUM 
                                        ,FBillNo  
                                        ,FID
                                        ,F_ora_PercaseQty  
                                        ,F_ORA_PRINTTIMES
                                        ,sum(F_ora_PercaseQty) as Fbarcodesum
                                        from ora_t_Cust100006 
                                        where F_ORA_PRODUCTINORDNUM = '{0}'and F_ORA_PRODUCTNUM ='{1}' group by  F_ORA_PRODUCTINORDNUM 
                                        ,F_ORA_PRODUCTNUM ,FBillNo  ,F_ora_PercaseQty, FID,F_ORA_PRINTTIMES ", FID, FMATERIALID);
                    DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);
                    DataTable dt = ds.Tables[0];

                    Entity entity = this.View.BillBusinessInfo.GetEntity("F_ora_Entity1");
                    //当条码存在时 给历史条码添加信息
                    if (dt.Rows.Count > 0)
                    {
                        decimal BarcodeSum = Convert.ToInt32(dt.Rows[0]["Fbarcodesum"]);
                        this.Model.SetValue("F_ora_BarcodeSumQty", BarcodeSum);//条码汇总数量
                        DynamicObjectCollection rows = this.Model.GetEntityDataObject(entity);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            //每一行的值定义成 DynamicObject类型
                            DynamicObject row = new DynamicObject(entity.DynamicObjectType);
                            //entity.SeqDynamicProperty.SetValue(row, i -1);
                            //第i行
                            row["F_ora_ProductionOrdNumE"] = dt.Rows[i]["F_ora_ProductinOrdNum"].ToString();
                            row["F_ora_MaterialidE"] = dt.Rows[i]["F_ora_ProductNum"].ToString();
                            row["F_ora_BarcodeEntry"] = dt.Rows[i]["FBillNo"].ToString();
                            row["F_ora_EntryBoxQtyEntry1"] = Math.Round(Convert.ToDouble(dt.Rows[i]["F_ora_PercaseQty"].ToString()));
                            row["F_ora_HBarcodeId"] = Convert.ToInt32(dt.Rows[i]["FID"].ToString());
                            row["F_ora_PrintedQty"] = Convert.ToInt32(dt.Rows[i]["F_ORA_PRINTTIMES"]);
                            //Convert.ToInt32(dt.Rows[i]["F_ora_PercaseQty"].ToString());
                            this.View.Model.CreateNewEntryRow(entity, -1);
                            this.Model.ClearNoDataRow();
                            rows.Add(row);
                        }
                        this.View.UpdateView("F_ora_Entity1");
                    }


                }
            }
        }

        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            //如果点击的是批量生成条码按钮
            if (e.Key.EqualsIgnoreCase("F_ora_BtnBatchCreateBarcode"))
            {
                decimal sumBarcodeQty = Convert.ToDecimal(this.Model.GetValue("F_ora_BarcodeSumQty"));
                
                if (Convert.ToDecimal(this.Model.GetValue("F_ora_PrintySumQty")) == decimal.Zero
                    || Convert.ToDecimal(this.Model.GetValue("F_ora_BoxQty")) == decimal.Zero)
                {
                    this.View.ShowMessage("每箱数量和本次打印数量需要必填。");
                    return;
                }

                if (this.Model.GetValue("F_ora_ProductDate").IsNullOrEmptyOrWhiteSpace())
                {
                    this.View.ShowMessage("生产日期需要必填。");
                    return;
                }
                if (Convert.ToDecimal(this.Model.GetValue("F_ora_PrintySumQty")) <Convert.ToDecimal(this.Model.GetValue("F_ora_BoxQty"))) 
                {
                    //this.View.ShowErrMessage("本次打印总数不能小于每卡板数量");
                    throw new Exception("本次打印总数不能小于每卡板数量");
                    //return;
                }

                if (sumBarcodeQty + Convert.ToDecimal(this.View.Model.GetValue("F_ora_PrintySumQty"))
                    > Convert.ToDecimal(this.View.OpenParameter.GetCustomParameter("qty")))
                {
                    this.View.ShowMessage("条码产品总数 > 生产订单数量，请问是否继续生成条码？", MessageBoxOptions.OKCancel, new Action<MessageBoxResult>(result =>
                    {
                        if (result == MessageBoxResult.Cancel)
                        {
                            // 用户选择了取消       
                            return;
                        }
                    }));
                }

                string SCDDDATE = Convert.ToString(this.View.OpenParameter.GetCustomParameter("scddDate"));//生产订单日期    
                string FID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("Fid"));//单据编号
                string UNIT = Convert.ToString(this.View.OpenParameter.GetCustomParameter("unit"));//单位
                string FMATERIALID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FMaterialId"));//物料num
                //string FMATERIALIDNAME = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FMATERIALID"));//物料name
                //string GGXH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("GGXH"));//规格型号
                string QTY = Convert.ToString(this.View.OpenParameter.GetCustomParameter("qty"));//数量
                string FMATERIALIDNUMBER = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FMATERIALIDNumber"));//物料name
                string UNITNAME = Convert.ToString(this.View.OpenParameter.GetCustomParameter("UNITNAME"));//数量
                string PrintSumQty = this.View.Model.GetValue("F_ora_PrintySumQty").ToString();//本次打印总数105
                string mxsl = Convert.ToString(this.View.Model.GetValue("F_ora_BoxQty"));//每箱数量20
                int moSeq = Convert.ToInt32(this.View.OpenParameter.GetCustomParameter("FSeq"));
                string FOrderNo = "";
                if (this.View.Model.GetValue("FOrderNo").IsNullOrEmptyOrWhiteSpace()==false)
                {
                    FOrderNo = this.View.Model.GetValue("FOrderNo").ToString();//销售订单号
                }

                string F_LLL_KHPH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_KHPH"));//F_LLL_KHPH客户批号
                string BOMID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("BOMID"));//BomID
                string BOMNUMBER = Convert.ToString(this.View.OpenParameter.GetCustomParameter("BOMNUMBER"));//BOMNUMBER
                string BOMNAME = Convert.ToString(this.View.OpenParameter.GetCustomParameter("BOMNAME"));//BOMNUMBER
                string TGFS = Convert.ToString(this.View.OpenParameter.GetCustomParameter("TGFS"));//TGFS套管方式
                string newTGFS = "";
                if (TGFS.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string sqlTGFS = string.Format("select FDATAVALUE from T_BAS_ASSISTANTDATAENTRY_L where FENTRYID = '{0}'", TGFS); //F_LLL_TGFS
                    DataSet dsTGFS = DBServiceHelper.ExecuteDataSet(this.Context, sqlTGFS);
                    DataTable dtTGFS = dsTGFS.Tables[0];
                    newTGFS = dtTGFS.Rows[0][0].ToString();
                }
                string F_LLL_HTH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_HTH"));//合同号
                string F_LLL_KHMaterialID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_KHMaterialID"));//客户物料编码
                string F_LLL_KHMaterialName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_KHMaterialName"));//客户物料名称
                string F_LLL_KH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("F_LLL_KH"));//客户
                string SCX = "";
                if (this.Model.GetValue("F_LLL_Machine").IsNullOrEmptyOrWhiteSpace()==false) 
                {
                    DynamicObject dy_scx = this.Model.GetValue("F_LLL_Machine") as DynamicObject;
                    SCX = dy_scx["Name"].ToString();
                }
                string BZ = "";
                if (this.Model.GetValue("FFrequency").IsNullOrEmptyOrWhiteSpace() == false) 
                {
                    DynamicObject dy_bz = this.Model.GetValue("FFrequency") as DynamicObject;
                    BZ = dy_bz["Name"].ToString();
                }

                decimal barcodeLine = Convert.ToDecimal(PrintSumQty) / Convert.ToDecimal(mxsl);//打印总数/卡板数
                decimal barcodeLinelast = Convert.ToDecimal(PrintSumQty) % Convert.ToDecimal(mxsl);
                int st = Convert.ToInt32(Math.Floor(barcodeLine));//单据体要循环多少行
                //string FBillNo = FID + FMATERIALID + lsh.ToString().PadLeft(3, '0');
                //string lshh = lsh.ToString().PadLeft(1, '0');
                string sql = string.Format(" /*dialect*/ select F_ORA_BILLNO from ora_t_Cust100006 where  F_ORA_PRODUCTINORDNUM = '{0}' and F_ORA_PRODUCTNUM = {1}", FID, FMATERIALID);
                DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);
                DataTable dt = ds.Tables[0];

                int serialNum = 0;
                if (dt.Rows.Count != 0)
                {
                    serialNum = Convert.ToInt32(dt.AsEnumerable().Max(r => Convert.ToInt32(r["F_ORA_BILLNO"])));
                    //throw new Exception(serialNum.ToString());
                }
                //如果该条码已经存在 则吧条码带出到条码信息 明细内
                Entity entity = this.View.BillBusinessInfo.GetEntity("F_ora_Entity");
                DynamicObjectCollection rows = this.Model.GetEntityDataObject(entity);
                DynamicObject row = new DynamicObject(entity.DynamicObjectType);

                for (int i = 0; i < st; i++)
                {
                    serialNum++;
                    string lshlsh = serialNum.ToString().PadLeft(3, '0');
                    string fbillno = createBarcode(FID, moSeq, FMATERIALIDNUMBER, lshlsh,FOrderNo);
                    this.View.Model.CreateNewEntryRow(entity.Key);

                    int idx = this.Model.GetEntryRowCount(entity.Key) - 1;

                    //throw new Exception(this.Model.GetEntryRowCount(entity.Key).ToString());
                    this.Model.SetValue("F_ora_Barcode", fbillno, idx);
                    this.Model.SetValue("F_ora_EntryBoxQty1", mxsl, idx);

                    JObject jo = new JObject();
                    jo.Add("F_ora_BarcodeDate", SCDDDATE);
                    jo.Add("F_ora_ProductinOrdNum", FID);
                    jo.Add("F_ora_Unit", UNITNAME);
                    jo.Add("F_ora_ProductNum", FMATERIALIDNUMBER);
                    jo.Add("F_ora_ProductOrdQty", QTY);
                    jo.Add("FBillNo", fbillno);
                    jo.Add("F_ora_PercaseQty", mxsl);
                    jo.Add("F_ora_BillNo", serialNum);
                    jo.Add("F_ORA_PRINTQTY", PrintSumQty);//本次打印数量
                    jo.Add("F_ora_MoSeq", moSeq);
                    jo.Add("F_ora_ProductDate", this.Model.GetValue("F_ora_ProductDate").ToString());
                    jo.Add("F_ora_SumBoxQty", st + (barcodeLinelast > 0 ? 1 : 0));
                    jo.Add("F_ora_BoxNo", i + 1);
                    //jo.Add("F_LLL_BOMBH",BOMID);
                    jo.Add("F_LLL_BOMBH", BOMNUMBER);
                    jo.Add("F_BLL_BOMMC", BOMNAME);
                    jo.Add("F_LLL_KHPH", F_LLL_KHPH);
                    jo.Add("F_LLL_TGFS", newTGFS);
                    jo.Add("F_LLL_HTH", F_LLL_HTH);
                    jo.Add("F_LLL_KHMaterialID", F_LLL_KHMaterialID);
                    jo.Add("F_LLL_KHMaterialName", F_LLL_KHMaterialName);
                    jo.Add("FSCX",SCX);
                    jo.Add("FBZ",BZ);
                    jo.Add("F_LLL_KH",F_LLL_KH);

                    sumBarcodeQty += Convert.ToDecimal(mxsl);
                    WebApiResultHelper result = new WebApiResultHelper(BatchBarcodeInventory.JsonAdd(jo.ToString(), "administrator", "kingdee123*", Context.DBId, "http://127.0.0.1:42428/K3Cloud/"));
                    //string result = Convert.ToString(BatchBarcodeInventory.JsonAdd(jo.ToString(), "刘桥中", "888888", Context.DBId, "http://127.0.0.1:42428/K3Cloud/"));
                    //this.View.ShowMessage(result);

                    if (result.IsSuccess)
                    {
                        this.Model.SetValue("F_ora_BarcodeId", result.FID, idx);

                    }
                    else
                    {
                        this.View.ShowMessage(result.Errors());
                    }
                }
               
                if (barcodeLinelast > 0)
                {
                    serialNum++;
                    this.View.Model.CreateNewEntryRow(entity.Key);
                    string lshlsh = serialNum.ToString().PadLeft(3, '0');
                    string fbillno = createBarcode(FID, moSeq, FMATERIALIDNUMBER, lshlsh, FOrderNo);

                    row["F_ora_EntryBoxQty1"] = barcodeLinelast.ToString();
                    this.Model.SetValue("F_ora_EntryBoxQty1", barcodeLinelast, this.Model.GetEntryRowCount(entity.Key) - 1);
                    this.Model.SetValue("F_ora_Barcode", fbillno, this.Model.GetEntryRowCount(entity.Key) - 1);

                    JObject jo = new JObject();
                    jo.Add("F_ora_BarcodeDate", SCDDDATE);
                    jo.Add("F_ora_ProductinOrdNum", FID);
                    jo.Add("F_ora_Unit", UNITNAME);
                    jo.Add("F_ora_ProductNum", FMATERIALIDNUMBER);
                    jo.Add("F_ora_ProductOrdQty", QTY);
                    jo.Add("FBillNo", fbillno);
                    jo.Add("F_ora_PercaseQty", barcodeLinelast);
                    jo.Add("F_ora_BillNo", serialNum);
                    jo.Add("F_ORA_PRINTQTY", PrintSumQty);//本次打印数量 
                    jo.Add("F_ora_MoSeq", moSeq);
                    jo.Add("F_ora_ProductDate", Convert.ToDateTime(this.Model.GetValue("F_ora_ProductDate")));
                    jo.Add("F_ora_SumBoxQty", st + (barcodeLinelast > 0 ? 1 : 0));
                    jo.Add("F_ora_BoxNo", st + (barcodeLinelast > 0 ? 1 : 0));
                    jo.Add("F_LLL_BOMBH", BOMNUMBER);
                    jo.Add("F_LLL_BOMMC", BOMNAME);
                    jo.Add("F_LLL_KHPH", F_LLL_KHPH);
                    jo.Add("F_LLL_TGFS", newTGFS);
                    jo.Add("F_LLL_HTH", F_LLL_HTH);
                    jo.Add("F_LLL_KHMaterialID", F_LLL_KHMaterialID);
                    jo.Add("F_LLL_KHMaterialName", F_LLL_KHMaterialName);
                    jo.Add("FSCX", SCX);
                    jo.Add("FBZ", BZ);
                    jo.Add("F_LLL_KH", F_LLL_KH);

                    sumBarcodeQty += Convert.ToDecimal(barcodeLinelast);
                    WebApiResultHelper result = new WebApiResultHelper(BatchBarcodeInventory.JsonAdd(jo.ToString(), "administrator", "kingdee123*", Context.DBId, "http://127.0.0.1:42428/K3Cloud/"));
                    //string result = Convert.ToString(BatchBarcodeInventory.JsonAdd(jo.ToString(), "刘桥中", "888888", Context.DBId, "http://127.0.0.1:42428/K3Cloud/"));
                    //this.View.ShowMessage(result);

                    if (result.IsSuccess)
                    {
                        this.Model.SetValue("F_ora_BarcodeId", result.FID, this.Model.GetEntryRowCount(entity.Key) - 1);

                    }
                    else
                    {
                        this.View.ShowMessage(result.Errors());
                    }
                    //this.Model.SetValue("F_ora_BarcodeId", result.FID, this.Model.GetEntryRowCount(entity.Key) - 1);

                    //PrintView(result.FID, "7ea88099-9273-4ee3-9312-747453c7b6fc", "");
                }

                this.Model.SetValue("F_ora_BarcodeSumQty", sumBarcodeQty);
                this.View.UpdateView(entity.Key);
            }

            else if (e.Key.EqualsIgnoreCase("F_ora_PrintBarcode"))
            {
                DynamicObject dy_TypeID = this.Model.GetValue("F_LLL_ChooseTD") as DynamicObject;
                string TypeID = dy_TypeID["FNumber"].ToString();
                string TYPEID2 = this.Model.GetValue("F_LLL_ChooseTD").ToString();
                //7ea88099-9273-4ee3-9312-747453c7b6fc
                string FTDID = this.Model.GetValue("F_LLL_ChooseTD").ToString();//套打ID
                Entity entity = this.View.BillBusinessInfo.GetEntity("F_ora_Entity");
                List<DynamicObject> rows = this.Model.GetEntityDataObject(entity).Where(r => Convert.ToBoolean(r["F_ora_IsPrint"])).ToList();

                if (rows.Count == 0)
                {
                    this.View.ShowMessage("请先选择要打印的条码");
                    return;
                }
                List<string> printBarcodeIds = new List<string>();
                foreach (var r in rows)
                {
                    printBarcodeIds.Add(r["F_ora_BarcodeId"].ToString());
                    //printBarcodeIds.Add(TypeID);
                }
                Dictionary<string, List<string>> barcodeidlist = new Dictionary<string, List<string>>();
                barcodeidlist.Add(TypeID, printBarcodeIds);

                this.View.ReturnToParentWindow(new FormResult(barcodeidlist));
                this.View.Close();
                
                
            }
            else if (e.Key.EqualsIgnoreCase("F_ora_PrintHistoryBarcode"))
            {
                DynamicObject dy_TypeID = this.Model.GetValue("F_LLL_ChooseTD") as DynamicObject;
                string TypeID = dy_TypeID["FNumber"].ToString();
                string TYPEID2 = this.Model.GetValue("F_LLL_ChooseTD").ToString();
                Entity entity = this.View.BillBusinessInfo.GetEntity("F_ora_Entity1");
                List<DynamicObject> rows = this.Model.GetEntityDataObject(entity).Where(r => Convert.ToBoolean(r["F_ora_IsPrintHistoryBarcode"])).ToList();

                if (rows.Count == 0)
                {
                    this.View.ShowMessage("请先选择要打印的历史条码");
                    return;
                }

                List<string> printBarcodeIds = new List<string>();

                foreach (var r in rows)
                {
                    printBarcodeIds.Add(r["F_ora_HBarcodeId"].ToString());
                    //printBarcodeIds.Add(TypeID);
                }
                Dictionary<string, List<string>> barcodeidlist = new Dictionary<string, List<string>>();
                barcodeidlist.Add(TypeID, printBarcodeIds);

                this.View.ReturnToParentWindow(new FormResult(barcodeidlist));
                this.View.Close();
            }
        }

        /// <summary>
        /// 生成条码
        /// </summary>
        /// <param name="moBillNo">生产订单号</param>
        /// <param name="seq">行号</param>
        /// <param name="matNumber">物料编码</param>
        /// <param name="no">流水号</param>
        /// <returns></returns>
        private string createBarcode(string moBillNo, int seq, string matNumber, string no,string FOrderNo)
        {
            // 条码规则：生产订单号.行号.物料编码.流水号
            if (FOrderNo != "") 
            { 
                return $"{matNumber}-{moBillNo}-{FOrderNo}-{no}";
            }
            else
            {
                return $"{matNumber}-{moBillNo}-{no}";
            }
        }
    }
}

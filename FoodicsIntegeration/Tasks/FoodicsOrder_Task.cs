using Foodics.NetSuite.Shared;
using Foodics.NetSuite.Shared.DAO;
using Foodics.NetSuite.Shared.Model;
using Microsoft.Build.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace FoodicsIntegeration.Tasks
{
    public class FoodicsOrder_Task : Foodics_BaseIntegration
    {
        public override void Get(string Subsidiary)
        {
            int Subsidiary_Id = Utility.ConvertToInt(ConfigurationManager.AppSettings[Subsidiary + "Netsuite.Subsidiary_Id"]);
            object fromDateObj = new GenericeDAO<Invoice>().GetLatestModifiedDate(Subsidiary_Id, "CreateDate");
            DateTime fromDate = new DateTime();
            if (fromDateObj == null)
            {
                fromDate = DateTime.Now;
            }
            else
            {
                fromDate = (DateTime)fromDateObj;
            }
            fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day).AddDays(-3);
            // DateTime fromDate = Utility.ConvertToDateTime(ConfigurationManager.AppSettings["InvoiceDate"]);
            while (fromDate <= DateTime.Now)
            {
                string MainURL = ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "orders?include=original_order,charges.charge,customer,branch,creator,discount,combos.combo_size.combo,combos.products,products.product,products.discount,products.options,products.options.modifier_option,payments.payment_method&filter[status]=4&filter[status]=5" + "&filter[business_date]=" + fromDate.ToString("yyyy-MM-dd");
                //string MainURL = ConfigurationManager.AppSettings[Subsidiary + "Foodics.ResetURL"] + "orders?include=original_order,charges.charge,customer,branch,creator,discount,combos.combo_size.combo,combos.products,products.product,products.discount,products.options,products.options.modifier_option,payments.payment_method&filter[status]=4&filter[status]=5" + "&filter[id]=30cf0e5e-8999-42fb-b600-8c2a5179eb85";
                string NextPage = MainURL;
                do
                {
                    var client = new RestClient(NextPage);
                    //client.Timeout = -1;
                    client.Timeout = 200000;
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Authorization", "Bearer " + ConfigurationManager.AppSettings[Subsidiary + "Foodics.Token"]);
                    var response = client.Execute<Dictionary<string, List<FoodicsOrder>>>(request);
                    if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
                    {
                        string content = response.Content;
                        var Jobj = JObject.Parse(content);
                        var nodes = Jobj.Descendants()
                       .OfType<JProperty>()
                       .Where(p => p.Name == "next")
                       .Select(p => p.Parent)
                       .ToList();
                        if (nodes.Count > 0)
                        {
                            FoodicsLinks objLinks = new FoodicsLinks();
                            try
                            {
                                objLinks = JsonConvert.DeserializeObject<FoodicsLinks>(JsonConvert.SerializeObject(nodes[0]));
                                NextPage = objLinks.next;
                                if (!string.IsNullOrEmpty(NextPage))
                                {
                                    int startIndex = NextPage.LastIndexOf("?") + 1;
                                    int endIndex = NextPage.Length - startIndex;
                                    string page = NextPage.Substring(startIndex, endIndex);
                                    NextPage = MainURL + "&" + page;
                                }
                                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "From Date " + fromDate.ToString("yyyy-MM-dd") + " Page:" + NextPage);
                            }
                            catch (Exception ex)
                            {
                                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message + " - Page index:" + NextPage);
                            }
                        }
                        foreach (var item in response.Data)
                        {
                            if (item.Key == "data")
                            {
                                if (item.Value.Count > 0)
                                    Generate_Save_NetSuiteLst(item.Value, Subsidiary_Id);
                            }
                        }
                    }


                } while (!string.IsNullOrEmpty(NextPage));
                fromDate = fromDate.AddDays(1);
            }


        }


        private void Generate_Save_NetSuiteLst(List<FoodicsOrder> OrderLstAll, int Subsidiary_Id)
        {
            try
            {
                int Exe_length = 10;
                int lstend = Exe_length;
                Setting objSetting = new GenericeDAO<Setting>().GetWhere("Subsidiary_Netsuite_Id=" + Subsidiary_Id).FirstOrDefault();
                if (OrderLstAll.Count > 0)
                {
                    for (int Index = 0; Index < OrderLstAll.Count; Index += Exe_length)
                    {
                        if (Index + Exe_length >= OrderLstAll.Count)
                            lstend = OrderLstAll.Count - Index;
                        List<FoodicsOrder> OrderLst = OrderLstAll.GetRange(Index, lstend);
                        if (OrderLst.Count > 0)
                        {
                            List<Foodics.NetSuite.Shared.Model.PaymentMethod> PaymentMethodLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.PaymentMethod>().GetAll();
                            List<Invoice> Invoicelst = new List<Invoice>();
                            List<InvoiceItem> InvoiceItemlst = new List<InvoiceItem>();
                            List<PaymentMethodEntity> PaymentMethodEntitylst = new List<PaymentMethodEntity>();
                            foreach (var Foodicsitem in OrderLst)
                            {
                                List<Foodics.NetSuite.Shared.Model.Invoice> invoiceLst = new GenericeDAO<Foodics.NetSuite.Shared.Model.Invoice>().GetWhere(" Foodics_Id = '" + Foodicsitem.id + "'");
                                if (invoiceLst.Count == 0 || (invoiceLst.Count > 0 && Utility.ConvertToInt(invoiceLst[0].Netsuite_Id.ToString()) <= 0))
                                {
                                    Invoice Netsuiteitem = new Invoice();
                                    //barcode
                                    Netsuiteitem.Foodics_Id = Foodicsitem.id;
                                    Netsuiteitem.Order_Status = Foodicsitem.status;
                                    Netsuiteitem.Subsidiary_Id = Subsidiary_Id;

                                    if (Foodicsitem.original_order != null && !string.IsNullOrEmpty(Foodicsitem.original_order.id))
                                    {
                                        Netsuiteitem.Original_Foodics_Id = Foodicsitem.original_order.id;
                                    }
                                    else
                                    {
                                        Netsuiteitem.Original_Foodics_Id = "0";
                                    }

                                    if (Foodicsitem.Customer != null && !string.IsNullOrEmpty(Foodicsitem.Customer.Id))
                                    {
                                        Customer obj = new GenericeDAO<Customer>().GetByFoodicsId(Foodicsitem.Customer.Id);
                                        if (obj != null && Utility.ConvertToInt(obj.Id) > 0)
                                        {
                                            Netsuiteitem.Customer_Id = Utility.ConvertToInt(obj.Id);
                                            Netsuiteitem.Customer_Netsuite_Id = obj.Netsuite_Id;
                                        }
                                    }
                                    else
                                    {
                                        Netsuiteitem.Customer_Id = 0;
                                        Netsuiteitem.Customer_Netsuite_Id = 0;
                                    }


                                    if (Foodicsitem.Branch != null && !string.IsNullOrEmpty(Foodicsitem.Branch.id))
                                    {
                                        Location obj = new GenericeDAO<Location>().GetByFoodicsId(Foodicsitem.Branch.id);
                                        Netsuiteitem.Location_Id = obj.Netsuite_Id;
                                    }
                                    else
                                    {
                                        Netsuiteitem.Location_Id = 0;
                                    }

                                    //Invoice Discount
                                    if (Foodicsitem.discount != null && !string.IsNullOrEmpty(Foodicsitem.discount.id))
                                    {
                                        Discount obj = new GenericeDAO<Discount>().GetByFoodicsId(Foodicsitem.discount.id);
                                        if (obj != null && obj.Netsuite_Id > 0)
                                            Netsuiteitem.Discount_Id = obj.Netsuite_Id;

                                    }
                                    //Foodics Source
                                    if (Foodicsitem.source > 0)
                                        Netsuiteitem.Source = Enum.GetName(typeof(InvoiceSource), Foodicsitem.source);
                                    //Foodics CreatedBY
                                    if (Foodicsitem.creator != null && !string.IsNullOrEmpty(Foodicsitem.creator.id))
                                    {
                                        Netsuiteitem.CreatedBy = Foodicsitem.creator.name;

                                    }
                                    Netsuiteitem.Invoice_Discount_Type = Foodicsitem.discount_type;
                                    Netsuiteitem.Total_Discount = (float)Foodicsitem.discount_amount;
                                    Netsuiteitem.Date = Foodicsitem.business_date;
                                    Netsuiteitem.CreateAt = Foodicsitem.created_at;
                                    Netsuiteitem.OpenAt = Foodicsitem.opened_at;
                                    Netsuiteitem.CloseAt = Foodicsitem.closed_at;
                                    Netsuiteitem.UpdateAt = Foodicsitem.updated_at;

                                    Netsuiteitem.Interval_Note = Foodicsitem.kitchen_notes + Foodicsitem.customer_notes;
                                    Netsuiteitem.Notes = Netsuiteitem.Interval_Note;

                                    Netsuiteitem.Paid = (float)Foodicsitem.total_price;
                                    Netsuiteitem.Net_Payable = (float)Foodicsitem.subtotal_price;

                                    Netsuiteitem.BarCode = Foodicsitem.reference;
                                    Netsuiteitem.Number = Foodicsitem.number.ToString();
                                    //Products
                                    foreach (var prodobj in Foodicsitem.Products)
                                    {
                                        GetProducts(objSetting, InvoiceItemlst, Foodicsitem, prodobj, "", "");
                                    }
                                    if (Foodicsitem.combos != null)
                                    {
                                        foreach (var Comboobj in Foodicsitem.combos)
                                        {
                                            foreach (var prodobj in Comboobj.Products)
                                            {
                                                GetProducts(objSetting, InvoiceItemlst, Foodicsitem, prodobj, Comboobj.combo_size.name, Comboobj.combo_size.combo.name);
                                            }
                                        }
                                    }
                                    if (Foodicsitem.charges != null)
                                    {
                                        foreach (var objCharges in Foodicsitem.charges)
                                        {
                                            Product objproduct = new Product();
                                            Products objProds = new Products();
                                            objProds.quantity = 1;
                                            objProds.unit_price = objCharges.amount;
                                            objProds.Product = objproduct;
                                            objProds.status = 3;
                                            foreach (var objCharge in objCharges.charge)
                                            {
                                                objproduct.id = objCharge.id;
                                                objproduct.name = objCharge.name;
                                                GetProducts(objSetting, InvoiceItemlst, Foodicsitem, objProds, "", "");
                                            }
                                        }
                                    }
                                    //payment methods
                                    foreach (var payobj in Foodicsitem.payments)
                                    {
                                        PaymentMethod PaymentMethodobj = PaymentMethodLst.Where(x => x.Foodics_Id == payobj.payment_method.id).FirstOrDefault();

                                        PaymentMethodEntity paymethod = new PaymentMethodEntity();

                                        paymethod.Foodics_Id = Foodicsitem.id;

                                        paymethod.Entity_Type = 1;
                                        paymethod.Amount = (float)payobj.amount;
                                        paymethod.Ref = payobj.payment_method.name;
                                        paymethod.Notes = payobj.payment_method.name;
                                        paymethod.Business_Date = payobj.Business_Date;
                                        if (PaymentMethodobj != null && PaymentMethodobj.Netsuite_Id > 0)
                                        {
                                            paymethod.Payment_Method = PaymentMethodobj.Name_En;
                                            paymethod.Payment_Method_Id = PaymentMethodobj.Netsuite_Id;
                                            paymethod.Payment_Method_Percentage = PaymentMethodobj.Percentage;
                                        }
                                        else
                                        {
                                            paymethod.Payment_Method = "Cash";
                                            paymethod.Payment_Method_Id = 1;
                                            paymethod.Payment_Method_Percentage = 0;
                                        }
                                        paymethod.Payment_Method_Type = 0;
                                        paymethod.Payment_Method_Type_Netsuite_Id = 0;
                                        PaymentMethodEntitylst.Add(paymethod);
                                    }
                                    Invoicelst.Add(Netsuiteitem);
                                }
                            }
                            new GenericeDAO<Invoice>().FoodicsIntegration(Invoicelst);
                            new GenericeDAO<Invoice>().InvoiceDetailsDelete(Invoicelst);
                            new GenericeDAO<InvoiceItem>().ListInsertOnly(InvoiceItemlst);
                            new GenericeDAO<PaymentMethodEntity>().ListInsertOnly(PaymentMethodEntitylst);
                            new CustomDAO().InvoiceRelatedUpdate();

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogDAO.Integration_Exception(LogIntegrationType.Error, this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error " + ex.Message);
            }
        }

        private static void GetProducts(Setting objSetting, List<InvoiceItem> InvoiceItemlst, FoodicsOrder Foodicsitem, Products prodobj, string ComboSize_Name, string Combo_Name)
        {
            List<Item> lstitemobj = new GenericeDAO<Item>().GetWhere("Foodics_Id = '" + prodobj.Product.id + "'").ToList();
            Item itemobj = new Item();
            if (lstitemobj.Count > 0)
                itemobj = lstitemobj.FirstOrDefault();

            InvoiceItem Netsuiteinvoiceitem = new InvoiceItem();
            Netsuiteinvoiceitem.Foodics_Id = Foodicsitem.id;
            Netsuiteinvoiceitem.FoodicsItem_Id = prodobj.Product.id;
            Netsuiteinvoiceitem.FoodicsTax = 0;
            Netsuiteinvoiceitem.Item_Id = 0;
            if (itemobj != null)
            {
                Netsuiteinvoiceitem.Item_Id = itemobj.Netsuite_Id;
                Netsuiteinvoiceitem.Item_Type = ((Item_Type)itemobj.Item_Type).ToString();
                //if (!string.IsNullOrEmpty(itemobj.Foodics_Item_Type_Name) && itemobj.Foodics_Item_Type_Name == nameof(FoodicsItem_Type.Product) && objSetting.TaxApplied)
                //if (!string.IsNullOrEmpty(itemobj.Foodics_Item_Type_Name) && itemobj.Foodics_Item_Type_Name == nameof(FoodicsItem_Type.Product) )
                if (!string.IsNullOrEmpty(itemobj.FoodicsTaxGroup_Id))
                {
                    Netsuiteinvoiceitem.FoodicsTax = objSetting.TaxRate;
                }

                //if (!string.IsNullOrEmpty(itemobj.FoodicsTaxGroup_Id))
                //Netsuiteinvoiceitem.FoodicsTax = 15;
            }
            Netsuiteinvoiceitem.Item_Name = prodobj.Product.name;
            Netsuiteinvoiceitem.Item_Code = prodobj.Product.sku;

            Netsuiteinvoiceitem.Quantity = prodobj.quantity;
            Netsuiteinvoiceitem.Amount = (float)prodobj.unit_price;

            Netsuiteinvoiceitem.Line_Discount_Amount = (float)prodobj.discount_amount;
            Netsuiteinvoiceitem.Is_Ingredients_Wasted = prodobj.is_ingredients_wasted;
            Netsuiteinvoiceitem.ProductStatus = prodobj.status;
            Netsuiteinvoiceitem.ComboSize_Name = ComboSize_Name;
            Netsuiteinvoiceitem.Combo_Name = Combo_Name;
            InvoiceItemlst.Add(Netsuiteinvoiceitem);
            if (prodobj.options != null)
            {
                foreach (var optionitem in prodobj.options)
                {
                    Item optionitemobj = new GenericeDAO<Item>().GetWhere("Foodics_Id = '" + optionitem.modifier_option.id + "'").FirstOrDefault();
                    InvoiceItem NetsuiteInvoiceOptionItem = new InvoiceItem();
                    NetsuiteInvoiceOptionItem.Foodics_Id = Foodicsitem.id;
                    NetsuiteInvoiceOptionItem.FoodicsItem_Id = optionitem.modifier_option.id;
                    if (optionitemobj != null && optionitemobj.Netsuite_Id > 0)
                    {
                        NetsuiteInvoiceOptionItem.Item_Id = optionitemobj.Netsuite_Id;
                        NetsuiteInvoiceOptionItem.Item_Type = ((Item_Type)optionitemobj.Item_Type).ToString();

                    }
                    else
                        NetsuiteInvoiceOptionItem.Item_Id = 0;

                    if (optionitemobj != null && !string.IsNullOrEmpty(optionitemobj.FoodicsTaxGroup_Id))
                    {
                        NetsuiteInvoiceOptionItem.FoodicsTax = objSetting.TaxRate;
                    }
                    //if (objSetting.TaxOptionApplied)
                    //{
                    //    NetsuiteInvoiceOptionItem.FoodicsTax = objSetting.TaxRate;
                    //}

                    NetsuiteInvoiceOptionItem.Item_Name = optionitem.modifier_option.name;
                    NetsuiteInvoiceOptionItem.Item_Code = optionitem.modifier_option.sku;
                    NetsuiteInvoiceOptionItem.Quantity = prodobj.quantity;
                    NetsuiteInvoiceOptionItem.Amount = (float)optionitem.unit_price;

                    NetsuiteInvoiceOptionItem.Line_Discount_Amount = 0;
                    NetsuiteInvoiceOptionItem.Is_Ingredients_Wasted = prodobj.is_ingredients_wasted;
                    NetsuiteInvoiceOptionItem.ProductStatus = prodobj.status;
                    InvoiceItemlst.Add(NetsuiteInvoiceOptionItem);
                }
            }
        }
        public override Int64 Set(string parametersArr)
        {
            return 0;
        }



    }
}



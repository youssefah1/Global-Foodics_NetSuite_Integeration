alter table [dbo].[InvoiceItem]
ADD Combo_Name VARCHAR(50) NULL,
 ComboSize_Name VARCHAR(50) NULL

 alter table [dbo].[Categories]
ADD CategoryType int NULL

alter table [dbo].[Categories]
add cogs_account int NULL,
inter_cogs_account int NULL,
income_account int NULL,
gainloss_account int NULL,
assetaccount int NULL,
income_intercompany_account int NULL,
price_variance_account int NULL,
cust_qty_variance_account int NULL,
cust_ex_rate_account int NULL,
customer_vari_account int NULL,
cust_vendor_account int NULL
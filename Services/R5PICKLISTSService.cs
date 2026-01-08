
using BE.Domains.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;

namespace BE.Services
{
    public class R5PICKLISTSService
    {
        public string Connection = "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;";
        public string WebUrl { get; set; } = "https://vimico.eam.vn";

        public class EAM_Helper
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string UrlAddress { get; set; }
            public string Tenant { get; set; }
            public string EAMID { get; set; }
            public string Connection { get; set; }

            public EAM_Helper(string username, string password, string urladdress, string tenant, string eamid, string connection)
            {
                this.Username = username;
                this.Password = password;
                this.UrlAddress = urladdress;
                this.Tenant = tenant;
                this.EAMID = eamid;
                this.Connection = connection;
            }

            public async Task<LoginReturnModel> Login()
            {
                List<string> cookies = new List<string>();

                using var client = new System.Net.Http.HttpClient();

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{this.UrlAddress}/web/base/login?eamid={this.EAMID}"),
                    Method = HttpMethod.Post,
                    Headers =
            {
                { "request-source", "XMLHTTP" },
                { "request-type", "XMLHTTP" },
            },//tenant=DNA&USER_FUNCTION_NAME=LOGIN&SYSTEM_FUNCTION_NAME=LOGIN&window=main_eam&userid=ADMIN&password=Cdvt%402021&forgotpassword=
                    Content = new StringContent($"USER_FUNCTION_NAME=LOGIN&SYSTEM_FUNCTION_NAME=LOGIN&window=eam_main&userid={this.Username}&password={this.Password}&tenant={this.Tenant}", Encoding.UTF8, "application/x-www-form-urlencoded")
                };

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                    {
                        cookies.AddRange(setCookieHeaders);
                    }
                    var responseContent = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine($"Response Content:\n{responseContent.Trim()}");
                    var model = System.Text.Json.JsonSerializer.Deserialize<LoginReturnModel>(responseContent.Trim().Replace("\'", "\""));
                    model.Cookies = cookies[0].Split(";")[0];
                    //model.Dump();
                    model.Helper = this;
                    model.UrlAddress = this.UrlAddress;
                    model.Connection = this.Connection;
                    return model;
                }
                else
                {
                    Console.WriteLine($"Failed with status code: {response.StatusCode}");
                    return null;
                }
            }
            public async Task<string> CallAPI(LoginReturnModel model, string url, string data, Dictionary<string, string> EditList = null)
            {
                BetterHttpClient.HttpClient client = new BetterHttpClient.HttpClient();
                client.Headers.Add("Cookie", model.Cookies);
                client.Headers.Add("Accept", "*/*");
                var queryString = data;
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);
                queryCollection["eamid"] = model.EamId;
                queryCollection["tenant"] = model.Tenant;

                if (EditList != null && EditList.Count > 0)
                {
                    EditList.ToList().ForEach(p =>
                    {
                        queryCollection[p.Key] = p.Value;
                    });
                }

                string datas = client.Post(url, queryCollection);
                string fixedJson = Ultis.FixJsonCasing(datas);
                return fixedJson;
            }
            public async Task Download(string url, string filePath, string cookie)
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Cookie", cookie);

                    using (var response = await client.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var fileStream = File.Create(filePath))
                            {
                                await stream.CopyToAsync(fileStream);
                            }
                        }
                    }
                }
            }
            public class LoginReturnModel
            {
                public LoginReturnModel()
                {
                    //Cookies = new List<string>();		
                }

                [JsonPropertyName("REQ_KEY")]
                public string ReqKey { get; set; }

                [JsonPropertyName("eamid")]
                public string EamId { get; set; }

                [JsonPropertyName("external_eamid")]
                public string ExternalEamId { get; set; }

                [JsonPropertyName("EWSLanguage")]
                public string EwsLanguage { get; set; }

                [JsonPropertyName("iscallerlogin")]
                public string IsCallerLogin { get; set; }

                [JsonPropertyName("isportallogin")]
                public string IsPortalLogin { get; set; }

                [JsonPropertyName("ISNAMEDUSER")]
                public string IsNamedUser { get; set; }

                [JsonPropertyName("tenant")]
                public string Tenant { get; set; }

                [JsonPropertyName("cookies")]
                public String Cookies { get; set; }

                public EAM_Helper Helper { get; set; }
                public string UrlAddress { get; set; }
                public string Connection { get; set; }
            }
            public static class Ultis
            {
                //public static void Oracle(string _connection)
                //{
                //    string connectionString = _connection;
                //    using (var connection = new OracleConnection(connectionString))
                //    {

                //        connection.Open();
                //    }
                //}
                public static async Task<dynamic> MsSql(string _connection, string cmd)
                {
                    string connectionString = _connection;
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        await connection.ExecuteAsync(cmd);
                        return null;
                    }
                }
                public static async Task<dynamic> RemovePrintAccess(string user)
                {
                    try
                    {
                        string cmd = "update R5SESSIONS set SES_REPORTLEVEL = 0 where SES_USER = 'ADMIN'";
                        await MsSql("Data Source=10.10.3.2,1444;User ID=EAM;Password=Ingr.123;Initial Catalog=EAMDB_TRAIN;app=LINQPad", cmd);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                }
                public static string FixJsonCasing(string json)
                {
                    //
                    // Parse the JSON string into a JObject
                    JObject obj = JObject.Parse(json);

                    // Create a new JObject to store the fixed JSON
                    JObject fixedObj = new JObject();

                    // Iterate over each property in the original JObject
                    foreach (var property in obj.Properties())
                    {
                        // Convert the property name to lowercase
                        string fixedKey = property.Name.ToLower();

                        // Add the property with the fixed key to the new JObject
                        fixedObj[fixedKey] = property.Value;
                    }

                    // Serialize the fixed JObject back to a JSON string
                    string fixedJson = JsonConvert.SerializeObject(fixedObj, Newtonsoft.Json.Formatting.Indented);
                    return fixedJson;
                }
                public static ICollection<KeyValuePair<string, string>> NameValueCollectionToICollection(NameValueCollection nvc)
                {
                    List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

                    foreach (string key in nvc.AllKeys)
                    {
                        foreach (var value in nvc.GetValues(key))
                        {
                            list.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }

                    return list;
                }
                public static List<JToken> GetGridData(string data)
                {
                    List<JToken> ListObject = new List<Newtonsoft.Json.Linq.JToken>();
                    JObject jsonObject = JObject.Parse(data);
                    foreach (var item in jsonObject["pagedata"]["grid"]["GRIDRESULT"]["GRID"]["DATA"])
                    {
                        ListObject.Add(item);
                    }
                    return ListObject;
                }
                public static string GetCurrentGridCount(string data)
                {
                    List<JToken> ListObject = new List<Newtonsoft.Json.Linq.JToken>();
                    JObject jsonObject = JObject.Parse(data);
                    var Counter = jsonObject["pagedata"]["grid"]["GRIDRESULT"]["GRID"]["METADATA"]["RECORDS"].ToString();
                    return Counter.Replace(".", "").ToString();
                }
                public static int GetGridCount(string data)
                {
                    List<JToken> ListObject = new List<Newtonsoft.Json.Linq.JToken>();
                    JObject jsonObject = JObject.Parse(data);
                    var Counter = jsonObject["pagedata"]["grid"]["GRIDRESULT"]["GRID"]["TOTAL-COUNT"].ToString();
                    //Counter.Dump("Total row");
                    return int.Parse(Counter.Replace(".", "").Replace(",", ""));
                }
            }
        }

        private static PartsResult ParseApiResponse2(string response, string successMessage, string failMessage)
        {
            try
            {
                var json = JObject.Parse(response);
                var pagedata = json["pagedata"];

                if (pagedata != null)
                {
                    // Kiểm tra lỗi (success = false)
                    bool? hasSuccessFlag = pagedata["success"]?.ToObject<bool>();
                    if (hasSuccessFlag == false)
                    {
                        string? message = pagedata["messages"]?.FirstOrDefault()?["msg"]?.ToString();
                        return new PartsResult
                        {
                            Success = false,
                            Message = message ?? failMessage,
                            ReceiptCode = null
                        };
                    }

                    // ⭐ LẤY RECEIPTCODE NẾU CÓ
                    string? receiptCode = pagedata["values"]?["receiptcode"]?.ToString();

                    if (pagedata["commentList"] != null || pagedata["values"] != null)
                    {
                        return new PartsResult
                        {
                            Success = true,
                            Message = successMessage,
                            ReceiptCode = receiptCode   // ⭐ TRẢ RECEIPTCODE VỀ
                        };
                    }
                }

                return new PartsResult
                {
                    Success = false,
                    Message = failMessage,
                    ReceiptCode = null
                };
            }
            catch (Exception ex)
            {
                return new PartsResult
                {
                    Success = false,
                    Message = $"Lỗi xử lý phản hồi: {ex.Message}",
                    ReceiptCode = null
                };
            }
        }

        private static Result ParseApiResponse(string response, string successMessage, string failMessage)
        {
            try
            {
                var json = JObject.Parse(response);
                var pagedata = json["pagedata"];

                if (pagedata != null)
                {
                    bool? hasSuccessFlag = pagedata["success"]?.ToObject<bool>();
                    if (hasSuccessFlag == false)
                    {
                        string? message = pagedata["messages"]?.FirstOrDefault()?["msg"]?.ToString();
                        return new Result
                        {
                            Success = false,
                            Message = message ?? failMessage
                        };
                    }

                    if (pagedata["commentList"] != null || pagedata["values"] != null)
                    {
                        return new Result
                        {
                            Success = true,
                            Message = successMessage
                        };
                    }
                }
                return new Result
                {
                    Success = false,
                    Message = failMessage
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Success = false,
                    Message = $"Lỗi xử lý phản hồi: {ex.Message}"
                };
            }
        }

        public class StoreApi
        {
            private EAM_Helper.LoginReturnModel model { get; set; }
            private string url = "/web/base/WSJOBS.PAR";
            public StoreApi(EAM_Helper.LoginReturnModel _model)
            {
                model = _model;
            }

            public async Task<List<Store>> GetAllStore()
            {
                var _url = "web/base/LOVPOP";
                var data = "popup=true&GRID_NAME=LVSTOR_PT&GRID_TYPE=LOV&REQUEST_TYPE=LOV.HEAD_DATA.STORED&LOV_TAGNAME=store&usagetype=lov&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=HDR&LOV_ALIAS_NAME_1=page_mode&LOV_ALIAS_VALUE_1=display&LOV_ALIAS_TYPE_1=text&LOV_ALIAS_NAME_2=control.org&LOV_ALIAS_VALUE_2=&LOV_ALIAS_TYPE_2=text&LOV_ALIAS_NAME_3=hide_storecode&LOV_ALIAS_VALUE_3=&LOV_ALIAS_TYPE_3=text&eamid=033c9c32-c87f-46c2-94b2-61d5b9745a0d&tenant=TRAIN";
                Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                paramss.Add("tenant", this.model.Tenant);
                paramss.Add("eamid", this.model.EamId);

                var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                JObject json = JObject.Parse(res.ToString());
                JArray resultList = json["pagedata"]["grid"]["GRIDRESULT"]["GRID"]["DATA"] as JArray;
                var results = resultList != null
                    ? JsonConvert.DeserializeObject<List<Store>>(resultList.ToString())
                    : new List<Store>();

                return results;
            }
        }


        public class PickListdb
        {
            private readonly string _connection;

            public PickListdb(string connection)
            {
                _connection = connection;
            }
            public async Task<List<R5PICKLISTS>> GetAllPICKLIST(int currentpage, int pagesize, string? search)
            {
                using var connection = new SqlConnection(_connection);

                int offset = (currentpage - 1) * pagesize;

                string sql = @"
                SELECT 
                    PIC_OBJECT AS storeorg,
                    PIC_CODE AS pickticketnum,
                    PIC_DESC AS description,
                    PIC_STORE AS store,
                    PIC_STATUS AS pickticketstatus,
                    UCO_DESC AS pickticketstatus_display,
                    PIC_REQUIRED AS daterequired,
                    PIC_CLASS AS class,
                    PIC_UDFNUM02 AS Quarter,
                    PIC_UDFNUM03 AS year
                FROM R5PICKLISTS a
                LEFT JOIN R5UCODES b
                    ON a.PIC_STATUS = b.UCO_CODE
                   AND b.UCO_ENTITY = 'PLST'
                WHERE PIC_CLASS = 'NCVT'
                AND (
                     NULLIF(LTRIM(RTRIM(@search)), '') IS NULL
                     OR PIC_OBJECT LIKE '%' + @search + '%'
                     OR PIC_CODE LIKE '%' + @search + '%'
                     OR PIC_DESC LIKE '%' + @search + '%'
                     OR PIC_STORE LIKE '%' + @search + '%'
                     OR PIC_STATUS LIKE '%' + @search + '%'
                     OR UCO_DESC LIKE '%' + @search + '%')
                ORDER BY PIC_CODE
                OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
                ";

                var data = await connection.QueryAsync<R5PICKLISTS>(sql, new
                {
                    search,
                    offset,
                    pagesize
                });

                return data.ToList();
            }



            public async Task<List<LoaiPhieu>> GetPhanLoai(int currentpage, int pagesize, string? search)
            {
                using var connection = new SqlConnection(_connection);

                int offset = (currentpage - 1) * pagesize;

                string sql = @"
                select 
                CLS_DESC as des_text,
                cls_code as class
                FROM R5CLASSES 
                WHERE CLS_ENTITY = 'PICK'
                AND (
                     NULLIF(LTRIM(RTRIM(@search)), '') IS NULL
                     OR CLS_DESC LIKE '%' + @search + '%'
                     OR cls_code LIKE '%' + @search + '%')
                ORDER BY CLS_DESC
                OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
                ";

                var data = await connection.QueryAsync<LoaiPhieu>(sql, new
                {
                    search,
                    offset,
                    pagesize
                });

                return data.ToList();
            }

            public async Task<List<Person>> GetPerson(int currentpage, int pagesize, string? search, string org)
            {
                using var connection = new SqlConnection(_connection);

                int offset = (currentpage - 1) * pagesize;

                string sql = @"
                select 
                PER_CODE,
                PER_DESC
                FROM R5PERSONNEL 
                WHERE per_org = @org
                AND (
                     NULLIF(LTRIM(RTRIM(@search)), '') IS NULL
                     OR PER_CODE LIKE '%' + @search + '%'
                     OR PER_DESC LIKE '%' + @search + '%')
                ORDER BY PER_CODE
                OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
                ";

                var data = await connection.QueryAsync<Person>(sql, new
                {
                    search,
                    org,
                    offset,
                    pagesize
                });

                return data.ToList();
            }

            public async Task<List<ky>> GetKy(
                int currentpage,
                int pagesize,
                string? search
            )
                        {
                            using var connection = new SqlConnection(_connection);

                            int offset = (currentpage - 1) * pagesize;

                            string sql = @"
                WITH data_cte AS
                (
                    SELECT
                        CAST(ROW_NUMBER() OVER (ORDER BY DES_CODE) AS VARCHAR(10)) AS CODE,
                        DES_TEXT AS des_text
                    FROM R5DESCRIPTIONS
                    WHERE
                        DES_TYPE = 'PICK'
                        AND DES_LANG = 'VI'
                        AND DES_CODE LIKE '%udfchar08%'
                        AND
                        (
                            NULLIF(LTRIM(RTRIM(@search)), '') IS NULL
                            OR DES_TEXT LIKE '%' + @search + '%'
                        )
                )
                SELECT
                    CODE,
                    des_text
                FROM data_cte
                ORDER BY CAST(CODE AS INT)
                OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
                ";

                var data = await connection.QueryAsync<ky>(sql, new
                {
                    search,
                    offset,
                    pagesize
                });

                return data.ToList();
            }



            public async Task<List<Part>> GetAllPartPopUp(int currentpage, int pagesize, string? search)
            {
                using var connection = new SqlConnection(_connection);

                int offset = (currentpage - 1) * pagesize;

                string sql = @"
                select 
                par_code as parcode,
                PAR_DESC as pardesc,
                PAR_UOM as uom
                FROM R5PARTS 
                WHERE
                (
                     NULLIF(LTRIM(RTRIM(@search)), '') IS NULL
                     OR par_code LIKE '%' + @search + '%'
                     OR PAR_DESC LIKE '%' + @search + '%'
                     OR PAR_UOM LIKE '%' + @search + '%')
                ORDER BY par_code
                OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
                ";

                var data = await connection.QueryAsync<Part>(sql, new
                {
                    search,
                    offset,
                    pagesize
                });

                return data.ToList();
            }

            public async Task<List<PartList>> GetPartList( int currentpage, int pagesize, string? search )
            {
                using var connection = new SqlConnection(_connection);

                int offset = (currentpage - 1) * pagesize;

                string sql = @"
                WITH data_cte AS
                (
                    SELECT
                        EVT_CODE AS workordercode,
                        EVT_DESC AS workordername,
                        PAR_DESC AS partdescription,
                        PAR_CODE AS partcode,
                        LTRIM(RTRIM(PAR_UOM)) AS uom,
                        ISNULL(SLTT, 0) AS actualqty,
                        ISNULL(MLP_QTY, 0) AS plannedqty,
                        CASE 
                            WHEN ROUND(PPR_PRICE, 2) = 0 THEN NULL 
                            ELSE ROUND(PPR_PRICE, 2) 
                        END AS unitprice,
                        ROUND(SLTT * PPR_PRICE, 2) AS totalamount
                    FROM R5EVENTS
                    INNER JOIN R5ACTIVITIES ON EVT_CODE = ACT_EVENT
                    INNER JOIN R5MATLISTS ON ACT_MATLIST = MTL_CODE
                    INNER JOIN R5MATLPARTS ON MTL_CODE = MLP_MATLIST
                    INNER JOIN R5PARTS ON PAR_CODE = MLP_PART AND PAR_ORG = MLP_PART_ORG
                    LEFT JOIN EAM_PRICE 
                        ON PAR_CODE COLLATE Latin1_General_CI_AS = PPR_PART COLLATE Latin1_General_CI_AS
                        AND PPR_MONTH = MONTH(EVT_SCHEDEND)
                        AND PPR_YEAR = YEAR(EVT_SCHEDEND)
                        AND PPR_ORG COLLATE Latin1_General_CI_AS = EVT_ORG
                    LEFT JOIN
                    (
                        SELECT TRL_EVENT, TRL_PART, -SUM(TRL_IO * TRL_QTY) AS SLTT
                        FROM R5TRANSLINES
                        GROUP BY TRL_EVENT, TRL_PART
                    ) TRL ON TRL.TRL_EVENT = EVT_CODE AND TRL.TRL_PART = PAR_CODE

                    UNION ALL

                    SELECT
                        EVT_CODE AS workordercode,
                        EVT_DESC AS workordername,
                        PAR_DESC AS partdescription,
                        PAR_CODE AS partcode,
                        LTRIM(RTRIM(PAR_UOM)) AS uom,
                        ISNULL(SLTT, 0) AS actualqty,
                        0 AS plannedqty,
                        CASE 
                            WHEN ROUND(PPR_AVGPRICE, 2) = 0 THEN NULL 
                            ELSE ROUND(PPR_AVGPRICE, 2) 
                        END AS unitprice,
                        ROUND(SLTT * PPR_AVGPRICE, 2) AS totalamount
                    FROM R5EVENTS
                    INNER JOIN
                    (
                        SELECT TRL_EVENT, TRL_PART, -SUM(TRL_IO * TRL_QTY) AS SLTT
                        FROM R5TRANSLINES
                        GROUP BY TRL_EVENT, TRL_PART
                    ) TRL ON TRL.TRL_EVENT = EVT_CODE
                    INNER JOIN R5PARTS ON PAR_CODE = TRL_PART
                    INNER JOIN R5PARTPRICES 
                        ON PAR_CODE = PPR_PART 
                        AND PPR_ORG COLLATE Latin1_General_CI_AS = EVT_ORG
                    WHERE PAR_CODE NOT IN
                    (
                        SELECT MLP_PART
                        FROM R5MATLISTS
                        INNER JOIN R5MATLPARTS ON MLP_MATLIST = MTL_CODE
                        INNER JOIN R5ACTIVITIES ON ACT_MATLIST = MTL_CODE
                        WHERE ACT_EVENT = EVT_CODE
                    )
                )
                SELECT *
                FROM data_cte
                WHERE
                (
                    NULLIF(LTRIM(RTRIM(@search)), '') IS NULL
                    OR workordercode LIKE '%' + @search + '%'
                    OR workordername LIKE '%' + @search + '%'
                    OR partcode LIKE '%' + @search + '%'
                    OR partdescription LIKE '%' + @search + '%'
                    OR uom LIKE '%' + @search + '%'
                )
                ORDER BY workordercode, partcode
                OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY;
                ";

                var data = await connection.QueryAsync<PartList>(
                    sql,
                    new
                    {
                        search,
                        offset,
                        pagesize
                    });

                return data.ToList();
            }
            public async Task<List<PartList>> GetPartListByWo(string wo)
            {
                using var connection = new SqlConnection(_connection);

                string sql = @"
                WITH data_cte AS
                (
                    SELECT
                        EVT_CODE AS workordercode,
                        EVT_DESC AS workordername,
                        PAR_DESC AS partdescription,
                        PAR_CODE AS partcode,
                        LTRIM(RTRIM(PAR_UOM)) AS uom,
                        ISNULL(SLTT, 0) AS actualqty,
                        ISNULL(MLP_QTY, 0) AS plannedqty,
                        CASE 
                            WHEN ROUND(PPR_PRICE, 2) = 0 THEN NULL 
                            ELSE ROUND(PPR_PRICE, 2) 
                        END AS unitprice,
                        ROUND(SLTT * PPR_PRICE, 2) AS totalamount
                    FROM R5EVENTS
                    INNER JOIN R5ACTIVITIES ON EVT_CODE = ACT_EVENT
                    INNER JOIN R5MATLISTS ON ACT_MATLIST = MTL_CODE
                    INNER JOIN R5MATLPARTS ON MTL_CODE = MLP_MATLIST
                    INNER JOIN R5PARTS ON PAR_CODE = MLP_PART AND PAR_ORG = MLP_PART_ORG
                    LEFT JOIN EAM_PRICE 
                        ON PAR_CODE COLLATE Latin1_General_CI_AS = PPR_PART COLLATE Latin1_General_CI_AS
                        AND PPR_MONTH = MONTH(EVT_SCHEDEND)
                        AND PPR_YEAR = YEAR(EVT_SCHEDEND)
                        AND PPR_ORG COLLATE Latin1_General_CI_AS = EVT_ORG
                    LEFT JOIN
                    (
                        SELECT TRL_EVENT, TRL_PART, -SUM(TRL_IO * TRL_QTY) AS SLTT
                        FROM R5TRANSLINES
                        GROUP BY TRL_EVENT, TRL_PART
                    ) TRL ON TRL.TRL_EVENT = EVT_CODE AND TRL.TRL_PART = PAR_CODE
                    WHERE EVT_CODE = @wo

                    UNION ALL

                    SELECT
                        EVT_CODE AS workordercode,
                        EVT_DESC AS workordername,
                        PAR_DESC AS partdescription,
                        PAR_CODE AS partcode,
                        LTRIM(RTRIM(PAR_UOM)) AS uom,
                        ISNULL(SLTT, 0) AS actualqty,
                        0 AS plannedqty,
                        CASE 
                            WHEN ROUND(PPR_AVGPRICE, 2) = 0 THEN NULL 
                            ELSE ROUND(PPR_AVGPRICE, 2) 
                        END AS unitprice,
                        ROUND(SLTT * PPR_AVGPRICE, 2) AS totalamount
                    FROM R5EVENTS
                    INNER JOIN
                    (
                        SELECT TRL_EVENT, TRL_PART, -SUM(TRL_IO * TRL_QTY) AS SLTT
                        FROM R5TRANSLINES
                        GROUP BY TRL_EVENT, TRL_PART
                    ) TRL ON TRL.TRL_EVENT = EVT_CODE
                    INNER JOIN R5PARTS ON PAR_CODE = TRL_PART
                    INNER JOIN R5PARTPRICES 
                        ON PAR_CODE = PPR_PART 
                        AND PPR_ORG COLLATE Latin1_General_CI_AS = EVT_ORG
                    WHERE EVT_CODE = @wo
                    AND PAR_CODE NOT IN
                    (
                        SELECT MLP_PART
                        FROM R5MATLISTS
                        INNER JOIN R5MATLPARTS ON MLP_MATLIST = MTL_CODE
                        INNER JOIN R5ACTIVITIES ON ACT_MATLIST = MTL_CODE
                        WHERE ACT_EVENT = EVT_CODE
                    )
                )
                SELECT *
                FROM data_cte
                ORDER BY partcode;
                ";

                var data = await connection.QueryAsync<PartList>(
                    sql,
                    new { wo }
                );

                return data.ToList();
            }

            public async Task<List<U5CSVT>> GetVatTuByWo(string wo)
            {
                using var connection = new SqlConnection(_connection);

                string sql = @"
                    SELECT *
                    FROM U5CSVT
                    WHERE PCV = @pcv
                ";

                var data = await connection.QueryAsync<U5CSVT>(
                    sql,
                    new { pcv = wo }
                );

                return data.ToList();
            }


            public async Task<List<PartList>> GetPartNotInPickListByWo(string wo)
            {
                using var connection = new SqlConnection(_connection);

                string sql = @"
                    WITH data_cte AS
                    (
                        SELECT
                            EVT_CODE AS workordercode,
                            EVT_DESC AS workordername,
                            PAR_DESC AS partdescription,
                            PAR_CODE AS partcode,
                            LTRIM(RTRIM(PAR_UOM)) AS uom,
                            ISNULL(SLTT, 0) AS actualqty,
                            ISNULL(MLP_QTY, 0) AS plannedqty,
                            CASE 
                                WHEN ROUND(PPR_PRICE, 2) = 0 THEN NULL 
                                ELSE ROUND(PPR_PRICE, 2) 
                            END AS unitprice,
                            ROUND(SLTT * PPR_PRICE, 2) AS totalamount
                        FROM R5EVENTS
                        INNER JOIN R5ACTIVITIES ON EVT_CODE = ACT_EVENT
                        INNER JOIN R5MATLISTS ON ACT_MATLIST = MTL_CODE
                        INNER JOIN R5MATLPARTS ON MTL_CODE = MLP_MATLIST
                        INNER JOIN R5PARTS ON PAR_CODE = MLP_PART AND PAR_ORG = MLP_PART_ORG
                        LEFT JOIN EAM_PRICE 
                            ON PAR_CODE COLLATE Latin1_General_CI_AS = PPR_PART COLLATE Latin1_General_CI_AS
                            AND PPR_MONTH = MONTH(EVT_SCHEDEND)
                            AND PPR_YEAR = YEAR(EVT_SCHEDEND)
                            AND PPR_ORG COLLATE Latin1_General_CI_AS = EVT_ORG
                        LEFT JOIN
                        (
                            SELECT TRL_EVENT, TRL_PART, -SUM(TRL_IO * TRL_QTY) AS SLTT
                            FROM R5TRANSLINES
                            GROUP BY TRL_EVENT, TRL_PART
                        ) TRL ON TRL.TRL_EVENT = EVT_CODE AND TRL.TRL_PART = PAR_CODE
                        WHERE EVT_CODE = @wo

                        UNION ALL

                        SELECT
                            EVT_CODE AS workordercode,
                            EVT_DESC AS workordername,
                            PAR_DESC AS partdescription,
                            PAR_CODE AS partcode,
                            LTRIM(RTRIM(PAR_UOM)) AS uom,
                            ISNULL(SLTT, 0) AS actualqty,
                            0 AS plannedqty,
                            CASE 
                                WHEN ROUND(PPR_AVGPRICE, 2) = 0 THEN NULL 
                                ELSE ROUND(PPR_AVGPRICE, 2) 
                            END AS unitprice,
                            ROUND(SLTT * PPR_AVGPRICE, 2) AS totalamount
                        FROM R5EVENTS
                        INNER JOIN
                        (
                            SELECT TRL_EVENT, TRL_PART, -SUM(TRL_IO * TRL_QTY) AS SLTT
                            FROM R5TRANSLINES
                            GROUP BY TRL_EVENT, TRL_PART
                        ) TRL ON TRL.TRL_EVENT = EVT_CODE
                        INNER JOIN R5PARTS ON PAR_CODE = TRL_PART
                        INNER JOIN R5PARTPRICES 
                            ON PAR_CODE = PPR_PART 
                            AND PPR_ORG COLLATE Latin1_General_CI_AS = EVT_ORG
                        WHERE EVT_CODE = @wo
                          AND PAR_CODE NOT IN
                          (
                              SELECT MLP_PART
                              FROM R5MATLISTS
                              INNER JOIN R5MATLPARTS ON MLP_MATLIST = MTL_CODE
                              INNER JOIN R5ACTIVITIES ON ACT_MATLIST = MTL_CODE
                              WHERE ACT_EVENT = EVT_CODE
                          )
                    )
                    SELECT cte.*
                    FROM data_cte cte
                    WHERE NOT EXISTS
                    (
                        SELECT 1
                        FROM U5CSVT p
                        WHERE p.PCV = @wo
                          AND p.PART = cte.partcode
                          AND ISNULL(p.PLAN_QTY, 0) = ISNULL(cte.plannedqty, 0)
                    )
                    ORDER BY cte.partcode;
                ";
                var data = await connection.QueryAsync<PartList>(
                    sql,
                    new { wo }
                );
                return data.ToList();
            }
            public async Task<int> GetMaxLineByCode(string code)
            {
                using var connection = new SqlConnection(_connection);

                string sql = @"
                    SELECT ISNULL(MAX(LINE), 0)
                    FROM U5CSVT
                    WHERE CODE = @code
                ";

                int maxLine = await connection.ExecuteScalarAsync<int>(
                    sql,
                    new { code }
                );

                return maxLine;
            }


            public async Task<string> InsertU5CsvtAsync(U5CsvtInsert dto)
            {
                using var connection = new SqlConnection(_connection);
                await connection.OpenAsync();                 // ✅ MỞ CONNECTION
                using var transaction = connection.BeginTransaction();

                try
                {
                    string sql = @"
                    INSERT INTO U5CSVT
                    (
                        CODE,
                        LINE,
                        PCV,
                        PART,
                        PART_DESC,
                        UOM,
                        PLAN_QTY
                    )
                    VALUES
                    (
                        @code,
                        @line,
                        @pcv,
                        @part,
                        @part_desc,
                        @uom,
                        @plan_qty
                    );
                ";

                    int rows = await connection.ExecuteAsync(
                        sql,
                        new
                        {
                            dto.code,
                            dto.line,
                            dto.pcv,
                            dto.part,
                            dto.part_desc,
                            dto.uom,
                            dto.plan_qty
                        },
                        transaction
                    );

                    transaction.Commit();
                    return rows > 0 ? "SUCCESS" : "NOT INSERTED";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return $"ERROR@ {ex.Message}";
                }
            }


            public async Task<Result> CreatePartInPickList(string code, string wo)
            {
                try
                {

                    var partNotInPickList = await GetPartNotInPickListByWo(wo);

                    if (partNotInPickList == null || !partNotInPickList.Any())
                    {
                        return new Result
                        {
                            Success = true,
                            Message = "No part to add"
                        };
                    }

                    int maxLine = await GetMaxLineByCode(code);

                    foreach (var obj in partNotInPickList)
                    {
                        maxLine++;

                        decimal planQty = decimal.Parse(obj.PlannedQty);

                        var dto = new U5CsvtInsert
                        {
                            code = code,
                            line = maxLine,
                            pcv = wo,
                            part = obj.PartCode,
                            part_desc = obj.PartDescription,
                            uom = obj.UOM,
                            plan_qty = planQty,
                        };

                        var rs = await InsertU5CsvtAsync(dto);

                        if (rs != "SUCCESS")
                        {
                            return new Result
                            {
                                Success = false,
                                Message = $"Failed at part {obj.PartCode}: {rs}"
                            };
                        }
                    }

                    return new Result
                    {
                        Success = true,
                        Message = "Create PickList Parts success"
                    };
                }
                catch (Exception ex)
                {
                    return new Result
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
        }


        public class PickListApi 
        {
            private EAM_Helper.LoginReturnModel model { get; set; }
            private string url = "/web/base/SSPICK.HDR";
            public PickListApi(EAM_Helper.LoginReturnModel _model)
            {
                model = _model;
            }


            


            public async Task<Result> CreatePart(PartCreate dto)
            {
                try
                {
                    var _url = "web/base/BSUDSC.TAB.insertrecord?pageaction=SAVE";
                    var data = "GRID_ID=100033&GRID_NAME=NCVT_U1&DATASPY_ID=100280&MADDON_FILTER_ALIAS_NAME_1=wspf_10_pcv&MADDON_FILTER_OPERATOR_1=CONTAINS&MADDON_FILTER_JOINER_1=AND&MADDON_FILTER_SEQNUM_1=1&MADDON_FILTER_VALUE_1=133864&MADDON_LPAREN_1=false&MADDON_RPAREN_1=false&SYSTEM_FUNCTION_NAME=SSPICK&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=U1&CHECK_CF_CHANGEFLAG=true&pickticketnum=11429&storeorg=LDV&store=PXLA1&wspf_10_px=&wspf_10_updated=&wspf_10_pcv=133864&wspf_10_createdby=&wspf_10_tt=&wspf_10_line=2006&wspf_10_part_ldesc=&wspf_10_code=11429&wspf_10_part_desc=C%C3%A2n%20%C4%91%C4%A9a%205kg&wspf_10_ncvt_code=&wspf_10_price=&wspf_10_updatedby=&wspf_10_qty=&wspf_10_tdcc=&wspf_10_uom=C%C3%A1i&wspf_10_plan_qty=6&wspf_10_part=015.0001&wspf_10_created=&webservicepromptcode=DUCSVT&processaction=insert&pagemode=display&can_insert=true&can_update=true&can_delete=true&recordid=&id=&PKID=11429%232006&eamid=1f68d9b5-c842-47f1-a016-51fed4b5e667&tenant=TRAIN";
                    Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                    paramss.Add("tenant", this.model.Tenant);
                    paramss.Add("eamid", this.model.EamId);
                    paramss.Add("MADDON_FILTER_VALUE_1", dto.wspf_10_pcv);
                    paramss.Add("pickticketnum", dto.pickticketnum);
                    paramss.Add("storeorg", dto.storeorg);
                    paramss.Add("store", dto.store);
                    paramss.Add("wspf_10_pcv", dto.wspf_10_pcv);
                    paramss.Add("wspf_10_line", dto.line);
                    paramss.Add("wspf_10_code", dto.pickticketnum);
                    paramss.Add("wspf_10_part_desc", dto.partdesc);
                    paramss.Add("wspf_10_uom", dto.uom);
                    paramss.Add("wspf_10_plan_qty", dto.qty);
                    paramss.Add("wspf_10_part", dto.partcode);
                    paramss.Add("PKID", $"{dto.pickticketnum}#{dto.line}");

                    var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                    return ParseApiResponse(res.ToString(), "Create success", "Create failed");
                }
                catch (Exception ex)
                {
                    return new Result
                    {
                        Success = false,
                        Message = $"Create failed: {ex.Message}"
                    };
                }
            }

            public async Task<PickTicket> GetPICKLISTByWO(string pickticketnum)
            {
                var _url = "web/base/SSPICK.HDR";
                var data = "SYSTEM_FUNCTION_NAME=SSPICK&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=HDR&pickticketnum=11442&storeorg=LDV&store=PXCDVT1&SCROLLROW=YES&ONLY_DATA_REQUIRED=true&eamid=15f5ba3e-e7fa-4db0-912d-bf896b6d7a49&tenant=TRAIN";
                Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                paramss.Add("tenant", this.model.Tenant);
                paramss.Add("eamid", this.model.EamId);
                paramss.Add("pickticketnum", pickticketnum);

                var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                JObject json = JObject.Parse(res.ToString());
                JObject values = json["pagedata"]?["values"] as JObject;

                var results = values != null
                    ? values.ToObject<PickTicket>()
                    : new PickTicket();
                return results;
            }

            public async Task<Result> CreatePICKLIST(R5PICKLISTSCreate dto)
            {
                try
                {
                    string transactionDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    var _url = "web/base/SSPICK.HDR?pageaction=SAVE";
                    var data = "SYSTEM_FUNCTION_NAME=SSPICK&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=HDR&CHECK_CF_CHANGEFLAG=true&equipmentorg=LDV&classorg=*&persontypefilter=&supplierorg=&install_showjob=NO&recordid=null&can_delete=true&pagemode=display&audittablename=R5PICKLISTS&storeorg=LDV&pickticketrstatus=U&workflowsummary=&can_insert=true&can_update=true&udfchar01=&udfdate04=&udfchar02=&udfdate03=&pickticketstatus=U&pickticketstatus_display=%C4%90ang%20th%E1%BB%B1c%20hi%E1%BB%87n&udfdate05=&udfdate02=01%2F08%2F2026%2000%3A00&udfdate01=&udfnote02=&udfnote01=&supplier=&udfchar09=250&udfnum04=&udfchkbox02=0&udfnum03=2026&udfchkbox03=0&udfnum02=3&udfchar07=&udfnum01=1&udfchar08=2&udfchkbox01=0&udfchar05=015&totalvalue=&dateapproved=&udfchar06=&udfchar03=&udfchar04=123&udfnum05=&udfchar12=&udfchar13=&udfchar10=&udfchar11=&udfchkbox04=0&udfchkbox05=0&approvedby=&daterequired=01%2F07%2F2026&equipment=LDV&udfchar18=&udfchar19=&job=&udfchar16=&workorder=&udfchar17=&udfchar14=&toemployee=&udfchar15=&udfchar23=&udfchar24=&udfchar21=&udfchar22=&activity=&udfchar20=&description=TEST%2071&originator=VIMICO-ADMIN&deladdress=&totalcurrency=VND&udfchar29=&udfchar27=&class=NCVT&udfchar28=&dateprinted=&udfchar25=&udfchar26=&udfchar30=&defaultapprover=&store=LDV&pickticketnum=&PKID=%40%5BEMPTY%5D%23LDV%23LDV&eamid=2c70c8e6-f70f-414f-9102-bcb134bbc20d&tenant=TRAIN";
                    Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                    paramss.Add("tenant", this.model.Tenant);
                    paramss.Add("eamid", this.model.EamId);
                    paramss.Add("equipmentorg", dto.org);
                    paramss.Add("storeorg", dto.org);
                    paramss.Add("udfdate02", dto.ngaybaocao);
                    paramss.Add("udfchar09", dto.loainhucau);
                    paramss.Add("udfnum03", dto.year);
                    paramss.Add("udfnum02", dto.quarter);
                    paramss.Add("udfnum01", dto.month);
                    paramss.Add("udfchar08", dto.ky);
                    paramss.Add("udfchar05", dto.canbophanxuong);
                    paramss.Add("udfchar04", dto.canbophongcodien);
                    paramss.Add("daterequired", DateTime.Now.ToString("dd/MM/yyyy"));
                    paramss.Add("equipment", dto.org);
                    paramss.Add("description", dto.description);
                    paramss.Add("store", dto.store);
                    paramss.Add("PKID", $"@[EMPTY]#{dto.store}#{dto.store}");

                    var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                    return ParseApiResponse(res.ToString(), "Create success", "Create failed");
                }
                catch (Exception ex)
                {
                    return new Result
                    {
                        Success = false,
                        Message = $"Create failed: {ex.Message}"
                    };
                }
            }


            public async Task<Result> UpdatePICKLIST(R5PICKLISTSUpdate dto)
            {
                try
                {
                    var rc = await GetPICKLISTByWO(dto.pickticketnum);
                    string transactionDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    var _url = "web/base/SSPICK.HDR?pageaction=SAVE";
                    var data = "SYSTEM_FUNCTION_NAME=SSPICK&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=HDR&CHECK_CF_CHANGEFLAG=true&equipmentorg=MDV&classorg=*&persontypefilter=&supplierorg=&install_showjob=&recordid=0&can_delete=true&pagemode=view&audittablename=R5PICKLISTS&storeorg=MDV&pickticketrstatus=U&workflowsummary=&can_insert=true&can_update=true&udfchar01=&udfdate04=&udfchar02=&udfdate03=&pickticketstatus=U&pickticketstatus_display=%C4%90ang%20th%E1%BB%B1c%20hi%E1%BB%87n&udfdate05=&udfdate02=01%2F31%2F2026%2000%3A00&udfdate01=&udfnote02=&udfnote01=&supplier=&udfchar09=250-500&udfnum04=&udfchkbox02=0&udfnum03=2026&udfchkbox03=0&udfnum02=2&udfchar07=&udfnum01=3&udfchar08=1&udfchkbox01=0&udfchar05=0001&totalvalue=0&dateapproved=&udfchar06=&udfchar03=&udfchar04=22&udfnum05=&udfchar12=&udfchar13=&udfchar10=&udfchar11=&udfchkbox04=0&udfchkbox05=0&approvedby=&daterequired=01%2F07%2F2026&equipment=MDV&udfchar18=&udfchar19=&job=&udfchar16=&workorder=&udfchar17=&udfchar14=&toemployee=&udfchar15=&udfchar23=&udfchar24=&udfchar21=&udfchar22=&activity=&udfchar20=&description=test%2017&originator=VIMICO-ADMIN&deladdress=&totalcurrency=VND&udfchar29=&udfchar27=&class=NCVT&udfchar28=&dateprinted=&udfchar25=&udfchar26=&udfchar30=&defaultapprover=&store=MDV&pickticketnum=11443&PKID=11443%23MDV%23MDV&eamid=2c70c8e6-f70f-414f-9102-bcb134bbc20d&tenant=TRAIN";
                    Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                    paramss.Add("tenant", this.model.Tenant);
                    paramss.Add("eamid", this.model.EamId);
                    paramss.Add("equipmentorg", dto.org);
                    paramss.Add("pickticketnum", dto.pickticketnum);
                    paramss.Add("pickticketstatus", dto.pickticketstatus);
                    paramss.Add("pickticketrstatus", dto.pickticketstatus);
                    paramss.Add("pickticketstatus_display", dto.pickticketstatus_display);
                    paramss.Add("storeorg", dto.org);
                    paramss.Add("udfdate02", dto.ngaybaocao);
                    paramss.Add("udfchar09", dto.loainhucau);
                    paramss.Add("udfnum03", dto.year);
                    paramss.Add("udfnum02", dto.quarter);
                    paramss.Add("udfnum01", dto.month);
                    paramss.Add("udfchar08", dto.ky);
                    paramss.Add("udfchar05", dto.canbophanxuong);
                    paramss.Add("udfchar04", dto.canbophongcodien);
                    paramss.Add("daterequired", DateTime.Now.ToString("dd/MM/yyyy"));
                    paramss.Add("equipment", dto.org);
                    paramss.Add("description", dto.description);
                    paramss.Add("recordid", rc.recordid);
                    paramss.Add("store", dto.store);
                    paramss.Add("PKID", $"@{dto.pickticketnum}#{dto.store}#{dto.store}");

                    var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                    return ParseApiResponse(res.ToString(), "Update success", "Update failed");
                }
                catch (Exception ex)
                {
                    return new Result
                    {
                        Success = false,
                        Message = $"Update failed: {ex.Message}"
                    };
                }
            }

            public async Task<Result> DeletePICKLIST(string pickticketnum)
            {
                try
                {

                    var obj = await GetPICKLISTByWO(pickticketnum);

                    var _url = "web/base/SSPICK.HDR?pageaction=DELETE";
                    var data = "SYSTEM_FUNCTION_NAME=SSPICK&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=HDR&equipmentorg=MDV&classorg=*&persontypefilter=&supplierorg=&install_showjob=&recordid=1&can_delete=true&pagemode=view&audittablename=R5PICKLISTS&storeorg=MDV&pickticketrstatus=U&workflowsummary=&can_insert=true&can_update=true&udfchar01=&udfdate04=&udfchar02=&udfdate03=&pickticketstatus=U&pickticketstatus_display=%C4%90ang%20th%E1%BB%B1c%20hi%E1%BB%87n&udfdate05=&udfdate02=01%2F31%2F2026%2000%3A00&udfdate01=&udfnote02=&udfnote01=&supplier=&udfchar09=250-500&udfnum04=&udfchkbox02=0&udfnum03=2026&udfchkbox03=0&udfnum02=2&udfchar07=&udfnum01=3&udfchar08=1&udfchkbox01=0&udfchar05=0001&totalvalue=0&dateapproved=&udfchar06=&udfchar03=&udfchar04=22&udfnum05=&udfchar12=&udfchar13=&udfchar10=&udfchar11=&udfchkbox04=0&udfchkbox05=0&approvedby=&daterequired=01%2F07%2F2026&equipment=MDV&udfchar18=&udfchar19=&job=&udfchar16=&workorder=&udfchar17=&udfchar14=&toemployee=&udfchar15=&udfchar23=&udfchar24=&udfchar21=&udfchar22=&activity=&udfchar20=&description=test%2017&originator=VIMICO-ADMIN&deladdress=&totalcurrency=VND&udfchar29=&udfchar27=&class=NCVT&udfchar28=&dateprinted=&udfchar25=&udfchar26=&udfchar30=&defaultapprover=&store=MDV&pickticketnum=11443&PKID=11443%23MDV%23MDV&eamid=1f68d9b5-c842-47f1-a016-51fed4b5e667&tenant=TRAIN";
                    Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                    paramss.Add("tenant", this.model.Tenant);
                    paramss.Add("eamid", this.model.EamId);
                    paramss.Add("pickticketnum", pickticketnum);
                    paramss.Add("equipmentorg", obj.equipmentorg);
                    paramss.Add("recordid", obj.recordid);
                    paramss.Add("pickticketrstatus", obj.pickticketrstatus);
                    paramss.Add("storeorg", obj.storeorg);
                    paramss.Add("pickticketstatus", obj.pickticketrstatus);
                    paramss.Add("pickticketstatus_display", obj.pickticketstatus_display);
                    paramss.Add("udfdate02", obj.udfdate02);
                    paramss.Add("udfchar09", obj.udfchar09);
                    paramss.Add("udfchkbox02", obj.udfchkbox02);
                    paramss.Add("udfnum03", obj.udfnum03);
                    paramss.Add("udfnum02", obj.udfnum02);
                    paramss.Add("udfnum01", obj.udfnum01);
                    paramss.Add("udfchar08", obj.udfchar08);
                    paramss.Add("udfchar05", obj.udfchar05);
                    paramss.Add("udfchar04", obj.udfchar04);
                    paramss.Add("daterequired", obj.daterequired);
                    paramss.Add("equipment", obj.equipment);
                    paramss.Add("description", obj.description);
                    paramss.Add("originator", obj.originator);
                    paramss.Add("totalcurrency", obj.totalcurrency);
                    paramss.Add("store", obj.store);
                    paramss.Add("PKID", $"{pickticketnum}#{obj.equipmentorg}#{obj.store}");

                    var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                    return ParseApiResponse(res.ToString(), "Delete success", "Delete failed");
                }
                catch (Exception ex)
                {
                    return new Result
                    {
                        Success = false,
                        Message = $"Delete failed: {ex.Message}"
                    };
                }
            }



            public async Task<Wspf10> GetPartById(string pickticketnum, string line)
            {
                var _url = "web/base/BSUDSC.TAB";
                var data = "SYSTEM_FUNCTION_NAME=SSPICK&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=U1&pickticketnum=&storeorg=&store=&webservicepromptcode=DUCSVT&wspf_10_code=11443&wspf_10_line=2&pagemode=view&processaction=get&REFRESH_GRID=false&ONLY_DATA_REQUIRED=true&eamid=15f5ba3e-e7fa-4db0-912d-bf896b6d7a49&tenant=TRAIN";
                Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                paramss.Add("tenant", this.model.Tenant);
                paramss.Add("eamid", this.model.EamId);
                paramss.Add("wspf_10_code", pickticketnum);
                paramss.Add("wspf_10_line", line);

                var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                JObject json = JObject.Parse(res.ToString());
                JObject values = json["pagedata"]?["values"] as JObject;

                var results = values != null
                    ? values.ToObject<Wspf10>()
                    : new Wspf10();
                return results;
            }


            public async Task<Result> UpdatePart(PartCreate dto)
            {
                try
                {
                    var rc = await GetPartById(dto.pickticketnum, dto.line);
                    string transactionDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    var _url = "web/base/BSUDSC.TAB.insertrecord?pageaction=SAVE";
                    var data = "GRID_ID=100033&GRID_NAME=NCVT_U1&DATASPY_ID=100280&SYSTEM_FUNCTION_NAME=SSPICK&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=U1&CHECK_CF_CHANGEFLAG=true&pickticketnum=11443&storeorg=MDV&store=MDV&wspf_10_px=LDV&wspf_10_updated=&wspf_10_pcv=&wspf_10_createdby=&wspf_10_tt=&wspf_10_line=2&wspf_10_part_ldesc=&wspf_10_code=11443&wspf_10_part_desc=Ph%E1%BB%9Bt%20bu%E1%BB%93ng%20rung%20xe%20lu%20rung%3A%2013V0209&wspf_10_ncvt_code=&wspf_10_price=2000&wspf_10_updatedby=&wspf_10_qty=2&wspf_10_tdcc=&wspf_10_uom=C%C3%A1i&wspf_10_plan_qty=2&wspf_10_part=104.0002&wspf_10_created=&webservicepromptcode=DUCSVT&processaction=insert&pagemode=display&can_insert=true&can_update=true&can_delete=true&recordid=&id=&PKID=11443%232&eamid=15f5ba3e-e7fa-4db0-912d-bf896b6d7a49&tenant=TRAIN";
                    Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                    paramss.Add("tenant", this.model.Tenant);
                    paramss.Add("eamid", this.model.EamId);
                    paramss.Add("pickticketnum", dto.pickticketnum);
                    paramss.Add("storeorg", dto.storeorg);
                    paramss.Add("store", dto.store);
                    paramss.Add("wspf_10_px", dto.store);
                    paramss.Add("wspf_10_line", dto.line);
                    paramss.Add("wspf_10_code", dto.pickticketnum);
                    paramss.Add("wspf_10_part_desc", dto.partdesc);
                    paramss.Add("wspf_10_qty", dto.qty);
                    paramss.Add("wspf_10_uom", dto.uom);
                    paramss.Add("wspf_10_part", dto.partcode);
                    paramss.Add("recordid", rc.recordid);
                    paramss.Add("PKID", $"{dto.pickticketnum}#{dto.line}");

                    var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                    return ParseApiResponse(res.ToString(), "Update success", "Update failed");
                }
                catch (Exception ex)
                {
                    return new Result
                    {
                        Success = false,
                        Message = $"Update failed: {ex.Message}"
                    };
                }
            }

            public async Task<Result> DeletePart(string pickticketnum, string line)
            {
                try
                {

                    var obj = await GetPartById( pickticketnum, line);

                    var _url = "web/base/BSUDSC.TAB.deleterecord?pageaction=DELETE";
                    var data = "GRID_ID=100033&GRID_NAME=NCVT_U1&DATASPY_ID=100280&SYSTEM_FUNCTION_NAME=SSPICK&USER_FUNCTION_NAME=NCVT&CURRENT_TAB_NAME=U1&pickticketnum=11443&storeorg=MDV&store=MDV&wspf_10_updated=01%2F07%2F2026%2023%3A04&wspf_10_createdby=VIMICO-ADMIN&recordid=1&wspf_10_line=1&wspf_10_code=11443&wspf_10_part_desc=C%C3%A2n%20%C4%91%C4%A9a%205kg&wspf_10_price=3000&wspf_10_updatedby=VIMICO-ADMIN&wspf_10_qty=3&wspf_10_uom=C%C3%A1i&wspf_10_plan_qty=2&wspf_10_part=015.0001&wspf_10_created=01%2F07%2F2026%2022%3A52&webservicepromptcode=DUCSVT&processaction=delete&pagemode=view&can_insert=true&can_update=true&can_delete=true&wspf_10_ncvt_code=&wspf_10_pcv=&wspf_10_part_ldesc=&wspf_10_tdcc=&wspf_10_tt=&wspf_10_px=&id=&PKID=11443%231&eamid=1f68d9b5-c842-47f1-a016-51fed4b5e667&tenant=TRAIN";
                    Dictionary<string, string> paramss = new System.Collections.Generic.Dictionary<string, string>();
                    paramss.Add("tenant", this.model.Tenant);
                    paramss.Add("eamid", this.model.EamId);
                    paramss.Add("pickticketnum", pickticketnum);
                    paramss.Add("line", line);
                    paramss.Add("storeorg", obj.wspf_10_px);
                    paramss.Add("store", obj.wspf_10_px);
                    paramss.Add("wspf_10_code", pickticketnum);
                    paramss.Add("wspf_10_line", line);
                    paramss.Add("recordid", obj.recordid);
                    paramss.Add("wspf_10_part_desc", obj.wspf_10_part_desc);
                    paramss.Add("wspf_10_price", obj.wspf_10_price);
                    paramss.Add("wspf_10_qty", obj.wspf_10_qty);
                    paramss.Add("wspf_10_uom", obj.wspf_10_uom);
                    paramss.Add("wspf_10_plan_qty", obj.wspf_10_plan_qty);
                    paramss.Add("wspf_10_part", obj.wspf_10_part);
                    paramss.Add("wspf_10_created", obj.wspf_10_created);
                    paramss.Add("PKID", $"{pickticketnum}#{line}");

                    var res = await model.Helper.CallAPI(model, model.UrlAddress + _url, data, paramss);
                    return ParseApiResponse(res.ToString(), "Delete success", "Delete failed");
                }
                catch (Exception ex)
                {
                    return new Result
                    {
                        Success = false,
                        Message = $"Delete failed: {ex.Message}"
                    };
                }
            }



        }
    }

    public class U5CsvtInsert
    {
        public string code { get; set; }
        public int line { get; set; }
        public string pcv { get; set; }
        public string part { get; set; }
        public string part_desc { get; set; }
        public string uom { get; set; }
        public decimal plan_qty { get; set; }
    }


    public class U5CSVT
    {
        public string CODE { get; set; }
        public string NDCV_CODE { get; set; }
        public string LINE { get; set; }
        public string PCV { get; set; }
        public string PART { get; set; }
        public string PART_DESC { get; set; }
        public string UOM { get; set; }
        public string PLANT_QTY { get; set; }
        public string PRICE { get; set; }
        public string QTY { get; set; }
        public string TDCC { get; set; }
        public string TT { get; set; }
    }

    public class PartList
    {
        public string WorkOrderCode { get; set; }
        public string WorkOrderName { get; set; }
        public string PartDescription { get; set; }
        public string PartCode { get; set; }
        public string UOM { get; set; }
        public string ActualQty { get; set; }
        public string PlannedQty { get; set; }
        public string UnitPrice { get; set; }
        public string TotalAmount { get; set; }
    }
    public class Wspf10
    {
        public string recordid { get; set; }
        public string wspf_10_line { get; set; }
        public string wspf_10_code { get; set; }
        public string wspf_10_part_desc { get; set; }
        public string wspf_10_price { get; set; }
        public string wspf_10_px { get; set; }
        public string wspf_10_qty { get; set; }
        public string wspf_10_uom { get; set; }
        public string wspf_10_plan_qty { get; set; }
        public string wspf_10_createdby { get; set; }
        public string wspf_10_part { get; set; }
        public string wspf_10_created { get; set; }
        public string webservicepromptcode { get; set; }
        public string wsptab { get; set; }
        public string cfgrouplist { get; set; }
        public string processaction { get; set; }
        public string pagemode { get; set; }
        public bool can_insert { get; set; }
        public bool can_update { get; set; }
        public bool can_delete { get; set; }
    }

    public class PartCreate
    {

        public string pickticketnum { get; set; }
        public string wspf_10_pcv { get; set; }
        public string storeorg { get; set; }
        public string store { get; set; }
        public string line { get; set; }
        public string partdesc { get; set; }
        public string qty { get; set; }
        public string uom { get; set; }
        public string partcode { get; set; }
    }

    public class Part
    {
        public string parcode { get; set; }
        public string pardesc { get; set; }
        public string uom { get; set; }
    }

    public class PickTicket
    {
        public string equipmentorg { get; set; }
        public string classorg { get; set; }
        public string persontypefilter { get; set; }
        public string supplierorg { get; set; }
        public string install_showjob { get; set; }
        public string recordid { get; set; }
        public bool can_delete { get; set; }
        public string pagemode { get; set; }
        public string audittablename { get; set; }
        public string storeorg { get; set; }
        public string pickticketrstatus { get; set; }
        public string workflowsummary { get; set; }
        public bool can_insert { get; set; }
        public bool can_update { get; set; }
        public string udfchar01 { get; set; }
        public string udfdate04 { get; set; }
        public string udfchar02 { get; set; }
        public string udfdate03 { get; set; }
        public PickTicketStatus pickticketstatus { get; set; }
        public string pickticketstatus_display { get; set; }
        public string udfdate05 { get; set; }
        public string udfdate02 { get; set; }
        public string udfdate01 { get; set; }
        public string udfnote02 { get; set; }
        public string udfnote01 { get; set; }
        public string supplier { get; set; }
        public string udfchar09 { get; set; }
        public string udfnum04 { get; set; }
        public string udfchkbox02 { get; set; }
        public string udfnum03 { get; set; }
        public string udfchkbox03 { get; set; }
        public string udfnum02 { get; set; }
        public string udfchar07 { get; set; }
        public string udfnum01 { get; set; }
        public string udfchar08 { get; set; }
        public string udfchkbox01 { get; set; }
        public string udfchar05 { get; set; }
        public string totalvalue { get; set; }
        public string dateapproved { get; set; }
        public string udfchar06 { get; set; }
        public string udfchar03 { get; set; }
        public string udfchar04 { get; set; }
        public string udfnum05 { get; set; }
        public string udfchar12 { get; set; }
        public string udfchar13 { get; set; }
        public string udfchar10 { get; set; }
        public string udfchar11 { get; set; }
        public string udfchkbox04 { get; set; }
        public string udfchkbox05 { get; set; }
        public string approvedby { get; set; }
        public string daterequired { get; set; }
        public string equipment { get; set; }
        public string udfchar18 { get; set; }
        public string udfchar19 { get; set; }
        public string job { get; set; }
        public string udfchar16 { get; set; }
        public string workorder { get; set; }
        public string udfchar17 { get; set; }
        public string udfchar14 { get; set; }
        public string toemployee { get; set; }
        public string udfchar15 { get; set; }
        public string udfchar23 { get; set; }
        public string udfchar24 { get; set; }
        public string udfchar21 { get; set; }
        public string udfchar22 { get; set; }
        public string activity { get; set; }
        public string udfchar20 { get; set; }
        public string description { get; set; }
        public string originator { get; set; }
        public string deladdress { get; set; }
        public string totalcurrency { get; set; }
        public string udfchar29 { get; set; }
        public string udfchar27 { get; set; }
        public string @class { get; set; }
        public string udfchar28 { get; set; }
        public string dateprinted { get; set; }
        public string udfchar25 { get; set; }
        public string udfchar26 { get; set; }
        public string woactdash { get; set; }
        public string udfchar30 { get; set; }
        public string defaultapprover { get; set; }
        public string store { get; set; }
        public string pickticketnum { get; set; }
    }

    public class PickTicketStatus
    {
        public bool dd { get; set; }
        public string selected { get; set; }
        public List<Option> option { get; set; }
    }

    public class Option
    {
        public string display { get; set; }
        public string value { get; set; }
    }

    public class R5PICKLISTSUpdate
    {
        public string org { get; set; }
        public string pickticketstatus { get; set; }
        public string pickticketstatus_display { get; set; }
        public string pickticketnum { get; set; }
        public string ngaybaocao { get; set; } // ngày báo cáo
        public string loainhucau { get; set; } // loại nhu cầu
        public string year { get; set; } // năm
        public string quarter { get; set; } // quý
        public string month { get; set; } // tháng
        public string ky { get; set; } // kỳ
        public string canbophanxuong { get; set; } // cán bộ phân xưởng
        public string canbophongcodien { get; set; } // cán bộ phòng cơ điện 
        public string description { get; set; } // mô tả
        public string store { get; set; } // phân xưởng
    }

    public class ky
    {
        public string CODE { get; set; }
        public string DES_TEXT { get; set; }
    }
    public class R5PICKLISTSCreate
    {
        public string org { get; set; }
        public string ngaybaocao { get; set; } // ngày báo cáo
        public string loainhucau { get; set; } // loại nhu cầu
        public string year { get; set; } // năm
        public string quarter { get; set; } // quý
        public string month { get; set; } // tháng
        public string ky { get; set; } // kỳ
        public string canbophanxuong { get; set; } // cán bộ phân xưởng
        public string canbophongcodien { get; set; } // cán bộ phòng cơ điện 
        public string description { get; set; } // mô tả
        public string store { get; set; } // phân xưởng
    }
    public class PartsResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ReceiptCode { get; set; }
    }

    public class Person
    {
        public string PER_CODE { get; set; }
        public string PER_DESC { get; set; }
    }
    public class Store
    {
        public string id { get; set; }
        public string description { get; set; }
        public string storeorg { get; set; }
        public string storeorgcurrency { get; set; }
        public string storecode { get; set; }
    }

    public class LoaiPhieu
    {
        public string des_text { get; set; }
        public string @class { get; set; }
    }

    public class R5PICKLISTS
    {
        public string storeorg { get; set; }
        public string pickticketnum { get; set; }
        public string description { get; set; }
        public string store { get; set; }
        public string pickticketstatus { get; set; }
        public string pickticketstatus_display { get; set; }
        public string daterequired { get; set; }
        public string @class { get; set; }
        public string Quarter { get; set; }
        public string year { get; set; }
    }
    public class Result
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}

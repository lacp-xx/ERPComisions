using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPCommissions.ImportExportUtil
{
    public class ImportFromVFP
    {
        string vfpConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\ERPFOXPRO\Data\EZCELLERP.DBC";
        string sqlConnectionString = @"Data Source=TOTALMOBILEP-PC\SQLEXPRESS;Initial Catalog=ERPCommissions;Integrated Security=True";

        private SqlConnection sqlDestinationConnection;
        private OleDbConnection vfpSourceConnection;

        //import data from ARCUST table in VFP to ARCUST table in SQLSERVER

        public void ImportExportTable(string tableName)
        {
            var createARCUST = "CREATE TABLE [dbo].[arcust]([custno] [varchar](50) NULL, [locid] [varchar](50) NULL,	[company] [varchar](150) NULL,	[address] [varchar](50) NULL,	[address1] [varchar](50) NULL,	[city] [varchar](50) NULL,	[state] [varchar](50) NULL,	[zipcode] [varchar](50) NULL,	[workphone] [varchar](50) NULL,	[faxphone] [varchar](50) NULL,	[ein] [varchar](50) NULL,	[entitydate] [varchar](50) NULL,	[fedex] [varchar](50) NULL,	[ups] [varchar](50) NULL,	[artype] [varchar](50) NULL,	[arstat] [varchar](50) NULL,	[contact] [varchar](50) NULL,	[signdate] [varchar](50) NULL,	[opendate] [varchar](50) NULL,	[closedate] [varchar](50) NULL,	[balance] [varchar](50) NULL,	[onorder] [varchar](50) NULL,	[creditlimit] [varchar](50) NULL,	[creditstatus] [varchar](50) NULL,	[royalty] [varchar](50) NULL,	[csaroyalty] [varchar](50) NULL,	[cooproyalty] [varchar](50) NULL,	[repname] [varchar](50) NULL,	[territory] [varchar](50) NULL,	[region] [varchar](50) NULL,	[email] [varchar](150) NULL,	[adduser] [varchar](150) NULL,	[adddate] [varchar](150) NULL,	[lastupdateuser] [varchar](50) NULL,	[lastupdatedate] [varchar](50) NULL,	[shpcompany] [varchar](150) NULL,	[shpaddress] [varchar](50) NULL,	[shpaddress1] [varchar](50) NULL,	[shpcity] [varchar](50) NULL,	[shpstate] [varchar](50) NULL,	[shpzipcode] [varchar](50) NULL,	[shpcontact] [varchar](50) NULL,	[shpworkphone] [varchar](50) NULL,	[shpfaxphone] [varchar](50) NULL,	[pricelevel] [varchar](50) NULL,	[smtraineddate] [varchar](50) NULL,	[rswtraineddate] [varchar](50) NULL,	[insiderepname] [varchar](50) NULL,	[popshipdate] [varchar](50) NULL,	[trainedby] [varchar](50) NULL,	[dba] [varchar](50) NULL,	[fmrdate] [varchar](50) NULL,	[fmrcalc] [varchar](50) NULL,	[fmrdetail] [varchar](50) NULL,	[warrantydays] [varchar](50) NULL,	[onquote] [varchar](50) NULL,	[language] [varchar](50) NULL,	[fmrenddate] [varchar](50) NULL,	[website] [varchar](50) NULL,	[dob] [varchar](50) NULL,	[blindship] [varchar](50) NULL,	[mobile] [varchar](50) NULL,	[masterdealer] [varchar](50) NULL,	[carrierdealercode] [varchar](50) NULL,	[country] [varchar](50) NULL,	[ismaster] [varchar](50) NULL,	[shipvia] [varchar](50) NULL,	[shpcountry] [varchar](50) NULL,	[creditdate] [varchar](50) NULL,	[creditscore] [varchar](50) NULL,	[creditreference] [varchar](50) NULL,	[bankrouting] [varchar](50) NULL,	[bankaccount] [varchar](50) NULL,	[mastercom] [varchar](50) NULL,	[masterspiff] [varchar](50) NULL,	[masterresidual] [varchar](50) NULL,	[mastercom1] [varchar](50) NULL,	[masterspiff1] [varchar](50) NULL,	[warrantylevel] [varchar](50) NULL,	[masterrefill] [varchar](50) NULL,	[masterrefill1] [varchar](50) NULL,	[ext] [varchar](50) NULL,	[ccshop] [varchar](50) NULL,	[rmaemail] [varchar](50) NULL)";
            //var insertARCUST = "INSERT INTO [dbo].[arcust]           ([custno]           ,[locid]           ,[company]           ,[address]           ,[address1]           ,[city]           ,[state]           ,[zipcode]           ,[workphone]           ,[faxphone]           ,[ein]           ,[entitydate]           ,[fedex]           ,[ups]           ,[artype]           ,[arstat]           ,[contact]           ,[signdate]           ,[opendate]           ,[closedate]           ,[balance]           ,[onorder]           ,[creditlimit]           ,[creditstatus]           ,[royalty]           ,[csaroyalty]           ,[cooproyalty]           ,[repname]           ,[territory]           ,[region]           ,[email]           ,[adduser]           ,[adddate]           ,[lastupdateuser]           ,[lastupdatedate]           ,[shpcompany]           ,[shpaddress]           ,[shpaddress1]           ,[shpcity]           ,[shpstate]           ,[shpzipcode]           ,[shpcontact]           ,[shpworkphone]           ,[shpfaxphone]           ,[pricelevel]           ,[smtraineddate]           ,[rswtraineddate]           ,[insiderepname]           ,[popshipdate]           ,[trainedby]           ,[dba]           ,[fmrdate]           ,[fmrcalc]           ,[fmrdetail]           ,[warrantydays]           ,[onquote]           ,[language]           ,[fmrenddate]           ,[website]           ,[dob]           ,[blindship]           ,[mobile]           ,[masterdealer]           ,[carrierdealercode]           ,[country]           ,[ismaster]           ,[shipvia]           ,[shpcountry]           ,[creditdate]           ,[creditscore]           ,[creditreference]           ,[bankrouting]           ,[bankaccount]           ,[mastercom]           ,[masterspiff]           ,[masterresidual]           ,[mastercom1]           ,[masterspiff1]           ,[warrantylevel]           ,[masterrefill]           ,[masterrefill1]           ,[ext]           ,[ccshop]           ,[rmaemail])     VALUES           (<custno, varchar(50),>           ,<locid, varchar(50),>           ,<company, varchar(150),>           ,<address, varchar(50),>           ,<address1, varchar(50),>           ,<city, varchar(50),>           ,<state, varchar(50),>           ,<zipcode, varchar(50),>           ,<workphone, varchar(50),>           ,<faxphone, varchar(50),>           ,<ein, varchar(50),>           ,<entitydate, varchar(50),>           ,<fedex, varchar(50),>           ,<ups, varchar(50),>           ,<artype, varchar(50),>           ,<arstat, varchar(50),>           ,<contact, varchar(50),>           ,<signdate, varchar(50),>           ,<opendate, varchar(50),>           ,<closedate, varchar(50),>           ,<balance, varchar(50),>           ,<onorder, varchar(50),>           ,<creditlimit, varchar(50),>           ,<creditstatus, varchar(50),>           ,<royalty, varchar(50),>           ,<csaroyalty, varchar(50),>           ,<cooproyalty, varchar(50),>           ,<repname, varchar(50),>           ,<territory, varchar(50),>           ,<region, varchar(50),>           ,<email, varchar(150),>           ,<adduser, varchar(150),>           ,<adddate, varchar(150),>           ,<lastupdateuser, varchar(50),>           ,<lastupdatedate, varchar(50),>           ,<shpcompany, varchar(150),>           ,<shpaddress, varchar(50),>          ,<shpaddress1, varchar(50),>           ,<shpcity, varchar(50),>           ,<shpstate, varchar(50),>           ,<shpzipcode, varchar(50),>           ,<shpcontact, varchar(50),>           ,<shpworkphone, varchar(50),>           ,<shpfaxphone, varchar(50),>           ,<pricelevel, varchar(50),>           ,<smtraineddate, varchar(50),>           ,<rswtraineddate, varchar(50),>           ,<insiderepname, varchar(50),>          ,<popshipdate, varchar(50),>           ,<trainedby, varchar(50),>           ,<dba, varchar(50),>           ,<fmrdate, varchar(50),>           ,<fmrcalc, varchar(50),>           ,<fmrdetail, varchar(50),>           ,<warrantydays, varchar(50),>           ,<onquote, varchar(50),>           ,<language, varchar(50),>           ,<fmrenddate, varchar(50),>           ,<website, varchar(50),>           ,<dob, varchar(50),>           ,<blindship, varchar(50),>           ,<mobile, varchar(50),>           ,<masterdealer, varchar(50),>           ,<carrierdealercode, varchar(50),>           ,<country, varchar(50),>           ,<ismaster, varchar(50),>           ,<shipvia, varchar(50),>           ,<shpcountry, varchar(50),>           ,<creditdate, varchar(50),>           ,<creditscore, varchar(50),>           ,<creditreference, varchar(50),>           ,<bankrouting, varchar(50),>           ,<bankaccount, varchar(50),>           ,<mastercom, varchar(50),>           ,<masterspiff, varchar(50),>           ,<masterresidual, varchar(50),>           ,<mastercom1, varchar(50),>           ,<masterspiff1, varchar(50),>           ,<warrantylevel, varchar(50),>           ,<masterrefill, varchar(50),>           ,<masterrefill1, varchar(50),>           ,<ext, varchar(50),>           ,<ccshop, varchar(50),>           ,<rmaemail, varchar(50),>)";
            var selectAllFromArcust = "select * from " + tableName;

            OleDbDataAdapter dataAdapter;

            DataTable data = new DataTable();

            using (vfpSourceConnection = new OleDbConnection(vfpConnectionString))
            {
                using (var command = vfpSourceConnection.CreateCommand())
                {
                    command.CommandText = selectAllFromArcust;
                    dataAdapter = new OleDbDataAdapter(command);
                    dataAdapter.Fill(data);
                }
                using (sqlDestinationConnection = new SqlConnection(sqlConnectionString))
                {
                    sqlDestinationConnection.Open();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlDestinationConnection))
                    {
                        bulkCopy.DestinationTableName = tableName;
                        try
                        {
                            // Write from the source to the destination.
                            bulkCopy.WriteToServer(data);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }

                    }
                }
            }
        }


        public void ImportSpiffCommissionsStructure(int carrierId)
        {
            string selectCommissions;
            switch (carrierId)
            {
                case 1:// Simple Mobile
                    selectCommissions = "SELECT simplespiffstructuredetail.spiff, simplespiffstructuredetail.mrcmax, simplespiffstructuredetail.mrcmin, simplespiffstructure.comid, simplespiffstructure.startdate, simplespiffstructure.enddate, rsw_simplemobile.custno FROM rsw_simplemobile INNER JOIN simplespiffstructure ON rsw_simplemobile.comid = simplespiffstructure.comid INNER JOIN simplespiffstructuredetail ON simplespiffstructure.iid = simplespiffstructuredetail.fk_com";
                    break;
                case 2:// Net10
                    selectCommissions = "SELECT net10spiffstructuredetail.spiff, net10spiffstructuredetail.mrcmax, net10spiffstructuredetail.mrcmin, net10spiffstructure.comid, net10spiffstructure.startdate, net10spiffstructure.enddate, rsw_net10.custno FROM rsw_net10 INNER JOIN net10spiffstructure ON rsw_net10.comid = net10spiffstructure.comid INNER JOIN net10spiffstructuredetail ON net10spiffstructure.iid = net10spiffstructuredetail.fk_com";
                    break;
                case 3:// Telcel
                    selectCommissions = "SELECT telcelspiffstructuredetail.spiff, telcelspiffstructuredetail.mrcmax, telcelspiffstructuredetail.mrcmin, telcelspiffstructure.comid, telcelspiffstructure.startdate, telcelspiffstructure.enddate, rsw_telcel.custno FROM rsw_telcel INNER JOIN telcelspiffstructure ON rsw_telcel.comid = telcelspiffstructure.comid INNER JOIN telcelspiffstructuredetail ON telcelspiffstructure.iid = telcelspiffstructuredetail.fk_com";
                    break;
                case 4: // Tracfone not exist structure
                    selectCommissions = "SELECT tracfonespiffstructuredetail.spiff, net10spiffstructuredetail.mrcmax, net10spiffstructuredetail.mrcmin, net10spiffstructure.comid, net10spiffstructure.startdate, net10spiffstructure.enddate, rsw_net10.custno FROM rsw_net10 INNER JOIN net10spiffstructure ON rsw_net10.comid = net10spiffstructure.comid INNER JOIN net10spiffstructuredetail ON net10spiffstructure.iid = net10spiffstructuredetail.fk_com";
                    break;
                case 5: // Red Pocket
                    selectCommissions = "SELECT redpocketspiffstructuredetail.spiff, redpocketspiffstructuredetail.mrcmax, redpocketspiffstructuredetail.mrcmin, redpocketspiffstructure.comid, redpocketspiffstructure.startdate, redpocketspiffstructure.enddate, rsw_redpocket.custno FROM rsw_redpocket INNER JOIN redpocketspiffstructure ON rsw_redpocket.comid = redpocketspiffstructure.comid INNER JOIN redpocketspiffstructuredetail ON redpocketspiffstructure.iid = redpocketspiffstructuredetail.fk_com";
                    break;
                case 6: // Page Plus
                    selectCommissions = "SELECT pageplusspiffstructuredetail.spiff, pageplusspiffstructuredetail.mrcmax, pageplusspiffstructuredetail.mrcmin, pageplusspiffstructure.comid, pageplusspiffstructure.startdate, pageplusspiffstructure.enddate, rsw_pageplus.custno FROM rsw_pageplus INNER JOIN pageplusspiffstructure ON rsw_pageplus.comid = pageplusspiffstructure.comid INNER JOIN pageplusspiffstructuredetail ON pageplusspiffstructure.iid = pageplusspiffstructuredetail.fk_com";
                    break;
                case 7: // H2O
                    selectCommissions = "SELECT h2ospiffstructuredetail.spiff, h2ospiffstructuredetail.mrcmax, h2ospiffstructuredetail.mrcmin, h2ospiffstructure.comid, h2ospiffstructure.startdate, h2ospiffstructure.enddate, rsw_h2o.custno FROM rsw_h2o INNER JOIN h2ospiffstructure ON rsw_h2o.comid = h2ospiffstructure.comid INNER JOIN h2ospiffstructuredetail ON h2ospiffstructure.iid = h2ospiffstructuredetail.fk_com";
                    break;
                default:
                    selectCommissions = "";
                    break;
            }

            var selectCustomerId = "SELECT id from Customers where CustNo = '{0}'";
            var insertPlanCommission = "INSERT INTO [dbo].[PlanCommissions] ([MinValue],[MaxValue],[PlanCommissionValue],[DealerCommissionValue],[StartDate],[EndDate],[PaymentCalculationType],[CommissionType],[CarrierId]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')";
            var selectScopeIdentity = "select SCOPE_IDENTITY() as Id";
            var insertCustomerPlanCommission = "INSERT INTO [dbo].[CustomerPlanCommission]([Customers_Id],[PlanCommissions_Id])VALUES({0},{1})";

            OleDbDataAdapter dataAdapter;

            DataTable data = new DataTable();
            OleDbDataReader reader;

            using (vfpSourceConnection = new OleDbConnection(vfpConnectionString))
            {
                using (var command = vfpSourceConnection.CreateCommand())
                {
                    vfpSourceConnection.Open();
                    command.CommandText = selectCommissions;
                    reader = command.ExecuteReader();
                }

                using (sqlDestinationConnection = new SqlConnection(sqlConnectionString))
                {
                    sqlDestinationConnection.Open();
                    while (reader.Read())
                    {
                        using (var command = sqlDestinationConnection.CreateCommand())
                        {
                            //select cust id
                            command.CommandText = String.Format(selectCustomerId, reader["custno"]);
                            var custId = command.ExecuteScalar();
                            if (custId != null)
                            {
                                // insert in plan commission
                                command.CommandText = String.Format(insertPlanCommission, reader["mrcmin"], reader["mrcmax"], 0, reader["spiff"], reader["startdate"], reader["enddate"], 1, 1, carrierId);
                                command.ExecuteNonQuery();
                                //select plan commission ID
                                command.CommandText = selectScopeIdentity;
                                var id = command.ExecuteScalar();
                                //insert relation (custumer - commission)
                                command.CommandText = String.Format(insertCustomerPlanCommission, custId, id);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    reader.Close();
                    sqlDestinationConnection.Close();
                    try
                    {
                        // Write from the source to the destination.
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
        }

        public void ImportResidualCommissionsStructure(int carrierId)
        {
            
            string selectCommissions;
            switch (carrierId)
            {
                case 1:
                    selectCommissions = "SELECT rsw_simplemobile.custno, rsw_simplemobile.residual FROM rsw_simplemobile";
                    break;
                case 2:
                    selectCommissions = "SELECT rsw_net10.custno, rsw_net10.residual FROM rsw_net10";
                    break;
                case 3:
                    selectCommissions = "SELECT rsw_telcel.custno, rsw_telcel.residual FROM rsw_telcel";
                    break;
                case 4:
                    selectCommissions = "SELECT rsw_tracfone.custno, rsw_tracfone.residual FROM rsw_tracfone";
                    break;
                case 5:
                    selectCommissions = "SELECT rsw_redpocket.custno, rsw_redpocket.residual FROM rsw_redpocket";
                    break;
                case 6:
                    selectCommissions = "SELECT rsw_pageplus.custno, rsw_pageplus.residual FROM rsw_pageplus";
                    break;
                case 7:
                    selectCommissions = "SELECT rsw_h2o.custno, rsw_h2o.residual FROM rsw_h2o";
                    break;
                default:
                    selectCommissions = "";
                    break;

            }
            
            var selectCustomerId = "SELECT id from Customers where CustNo = '{0}'";
            var insertPlanCommission = "INSERT INTO [dbo].[PlanCommissions] ([MinValue],[MaxValue],[PlanCommissionValue],[DealerCommissionValue],[StartDate],[EndDate],[PaymentCalculationType],[CommissionType],[CarrierId]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')";
            var selectScopeIdentity = "select SCOPE_IDENTITY() as Id";
            var insertCustomerPlanCommission = "INSERT INTO [dbo].[CustomerPlanCommission]([Customers_Id],[PlanCommissions_Id])VALUES({0},{1})";

            OleDbDataAdapter dataAdapter;

            DataTable data = new DataTable();
            OleDbDataReader reader;

            using (vfpSourceConnection = new OleDbConnection(vfpConnectionString))
            {
                using (var command = vfpSourceConnection.CreateCommand())
                {
                    vfpSourceConnection.Open();
                    command.CommandText = selectCommissions;
                    reader = command.ExecuteReader();
                }

                using (sqlDestinationConnection = new SqlConnection(sqlConnectionString))
                {
                    sqlDestinationConnection.Open();
                    while (reader.Read())
                    {
                        using (var command = sqlDestinationConnection.CreateCommand())
                        {
                            //select cust id
                            command.CommandText = String.Format(selectCustomerId, reader["custno"]);
                            var custId = command.ExecuteScalar();
                            if (custId != null)
                            {
                                // insert in plan commission
                                command.CommandText = String.Format(insertPlanCommission, 0, 200, 0, reader["residual"], "3/3/2010", "3/3/2020", 2, 2, carrierId); //Dates are bad in EZCell year 1898
                                command.ExecuteNonQuery();
                                //select plan commission ID
                                command.CommandText = selectScopeIdentity;
                                var id = command.ExecuteScalar();
                                //insert relation (custumer - commission)
                                command.CommandText = String.Format(insertCustomerPlanCommission, custId, id);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    reader.Close();
                    sqlDestinationConnection.Close();
                    try
                    {
                        // Write from the source to the destination.
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
        }
    }
}

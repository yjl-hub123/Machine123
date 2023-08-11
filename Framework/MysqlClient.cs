using HelperLibrary;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Reflection.Emit;
using MySqlX.XDevAPI.Relational;

namespace Machine.Framework
{
    public class MysqlClient
    {
        private bool bConnect;                                 //连接标志
        private MySqlConnection MyConnect;                     //数据库句柄
        public string sError;                                  //数据库错误信息
        private readonly object DBLock;                        //数据库锁
        private string ConnectionString;
        class SqlNameDbType
        {
            public string name;
            public SqlDbType type;
        }
        Dictionary<Type, SqlNameDbType> Map = new Dictionary<Type, SqlNameDbType> {
            {typeof(int), new SqlNameDbType{ name = "int", type = SqlDbType.Int } },
            {typeof(string), new SqlNameDbType{ name = "text", type = SqlDbType.Text } },
            {typeof(DateTime), new SqlNameDbType{ name = "datetime", type = SqlDbType.Time} }

        };

        public List<MysqlTableAbs> TableLabelInfo; //表信息

        public MysqlClient()
        {
            TableLabelInfo = new List<MysqlTableAbs>();
            bConnect = false;
            sError = "";
            DBLock = new object();
        }

        public void AddTableStructure<T>(string name = null)
        {
            var Ttype = typeof(T);
            name = string.IsNullOrEmpty(name) ? Ttype.Name : name;

            // 创建表
            var tableInfo = new MysqlTableAbs();


            foreach (var propinfo in Ttype.GetProperties())
            {
                if (!Map.TryGetValue(propinfo.PropertyType, out var dbdata))
                    continue;
                tableInfo.Columns.Add(new TableColumn()
                {

                    Field = propinfo.Name,
                    Type = dbdata.name
                });
                tableInfo.TableName = name;
            }
            TableLabelInfo.Add(tableInfo);
        }

        /// <summary>
        /// 设置表信息
        /// </summary>
        /// <param name="sTableName"></param>
        /// <param name="LabelInfo"></param>
        public void SetTableLebelInfo(MysqlTableAbs mysqlTableAbs)
        {
            this.TableLabelInfo.Add(mysqlTableAbs);
        }
        /// <summary>
        /// 打开/连接数据库
        /// </summary>
        /// <param name="dBName"></param>
        /// <returns></returns>
        public bool OpenDB(string dBName, string user, string passworld, string ip, string prot)
        {
            MyConnect = new MySqlConnection($"Data Source={ip};Persist Security Info=yes; UserId={user}; PWD={passworld}");

            try
            {
                //打开通道
                MyConnect.Open();
            }
            catch (Exception ex)
            {
                MyConnect.Close();
                ShowMsgBox.ShowDialog(ex.Message, MessageType.MsgMessage);
                return false;
            }

            //创建数据库
            if (!CreateDataBase(dBName))
            {
                return false;
            }

            //创建连接字符串
            string connetStr = $"server={ip};port={prot};user={user};password={passworld}; database={dBName};Charset=utf8;";

            ConnectionString = connetStr;

            //创建connection对象
            MyConnect = new MySqlConnection(connetStr);

            try
            {
                //打开创建的数据库
                MyConnect.Open();
            }
            catch (Exception ex)
            {
                MyConnect.Close();
                ShowMsgBox.ShowDialog(ex.Message, MessageType.MsgMessage);
                return false;
            }
            bConnect = true;

            //建表
            if (!CreateTable())
            {
                return false;
            }

            return true;
        }


        public void ReadIni(out string user, out string passworld, out string dbNamem, out string ip, out string port, string sec = "Mysql", string path = null)
        {
            #region 读取数据库账号密码
            path = string.IsNullOrEmpty(path) ? Def.GetAbsPathName(Def.MachineCfg) : path;
            user = IniFile.ReadString(sec, nameof(user), "", path);
            passworld = IniFile.ReadString(sec, nameof(passworld), "", path);
            dbNamem = IniFile.ReadString(sec, nameof(dbNamem), "", path);
            ip = IniFile.ReadString(sec, nameof(ip), "", path);
            port = IniFile.ReadString(sec, nameof(port), "", path);
            IniFile.WriteString(sec, nameof(user), user, path);
            IniFile.WriteString(sec, nameof(passworld), passworld, path);
            IniFile.WriteString(sec, nameof(dbNamem), dbNamem, path);
            IniFile.WriteString(sec, nameof(ip), ip, path);
            IniFile.WriteString(sec, nameof(port), port, path);

            #endregion
        }
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="sDBName"></param>
        /// <returns></returns>
        public bool CreateDataBase(string sDBName)
        {
            string strCreateDataBase = string.Format("CREATE DATABASE IF NOT EXISTS {0} DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci", (object)sDBName);

            MySqlCommand mySqlCommand = new MySqlCommand(strCreateDataBase, MyConnect);
            try
            {
                mySqlCommand.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                ShowMsgBox.ShowDialog(ex.Message, MessageType.MsgMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断开数据库连接
        /// </summary>
        public void CloseDB()
        {
            if (MyConnect != null)
            {
                bConnect = false;
                MyConnect.Close();
            }
        }

        /// <summary>
        /// 判断数据库是否连接
        /// </summary>
        public bool IsOpen()
        {
            return bConnect;
        }

        /// <summary>
        /// 处理数据库操作
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        public bool CustomDBQuery(string sSql)
        {
            if (string.IsNullOrEmpty(sSql) || !IsOpen() || ConnectionString == null)
            {
                return false;
            }
            return RunMysql(connection =>
            {
                MySqlCommand cmd = new MySqlCommand(sSql, connection);
                cmd.ExecuteNonQuery();
            });


        }
        private bool RunMysqlCommand(Action<MySqlCommand> action)
        {
            return RunMysql(mysqlconnect =>
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = mysqlconnect;
                    action(cmd);
                }
            });
        }
        private bool RunMysql(Action<MySqlConnection> action)
        {
            if (!IsOpen() || ConnectionString == null)
            {
                return false;
            }

            lock (DBLock)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        action.Invoke(connection);
                        connection.Close();
                    }
                }
                catch (MySqlException ex)
                {
                    sError = string.Format("sql执行失败,{0}", (object)ex.Message);
                    //ShowMsgBox.ShowDialog(sError, MessageType.MsgMessage);
                    return false;
                }
            }

            return true;
        }




        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool InsterInto(string table, IEnumerable<object> values)
        {
            return InsterInto(table, values.ToArray());
        }
        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool InsterInto(string table, object value)
        {
            return InsterInto(table, new object[] { value });

        }
        public bool InsterInto(object obj) => InsterInto(obj.GetType().Name, new object[] { obj });



        private bool InsterInto(string table, object[] values)
        {
            if (values.Length == 0) return true;
            // 构建基本的 INSERT INTO 语句
            var sql = new StringBuilder($"INSERT INTO {table} VALUES");

            // 使用 LINQ 查询来获取 T 类型的所有属性
            var properties = values[0].GetType().GetProperties();

            for (int i = 0; i < values.Length; i++)
            {
                // 提取属性值
                var propertyValues = properties.Select(p => $"@{p.Name}{i}");

                // 添加到 INSERT INTO 语句中
                sql.Append("(" + string.Join(",", propertyValues) + "),");
            }

            // 去除末尾的逗号
            sql = sql.Remove(sql.Length - 1, 1);

            return RunMysqlCommand(cmd =>
            {
                for (int i = 0; i < values.Length; i++)
                    foreach (var propinfo in properties)
                        cmd.Parameters.Add(new MySqlParameter($"@{propinfo.Name}{i}", propinfo.GetValue(values[i])));
                cmd.CommandText = sql.ToString();
                cmd.ExecuteNonQuery();
            });


        }
        private string Conversion(PropertyInfo info, object value)
        {
            var res = info.GetValue(value);
            if (info.PropertyType == typeof(DateTime))
            {
                res = (res as DateTime?).Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            }
            if (info.PropertyType == typeof(string) || info.PropertyType == typeof(DateTime))
            {
                res = $"'{res}'";
            }
            return res.ToString();

        }



        //private bool CheckDatabaseTables(KeyValuePair<string, Dictionary<string, string>>)
        //{

        //}
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="strError"></param>
        /// <returns></returns>
        public bool CreateTable()
        {
            lock (DBLock)
            {
                foreach (var keyValuePair1 in this.TableLabelInfo)
                {
                    bool rest = false;
                    RunMysql(conn => rest = conn.GetSchema("Tables", new string[] { null, null, keyValuePair1.TableName }).Rows.Count > 0);
                    if (rest)
                    {
                        var Tablestruct = Select<TableColumn>($"DESCRIBE {keyValuePair1.TableName}");
                        if (keyValuePair1.ComparisonField(Tablestruct))
                            continue;
                        CustomDBQuery($"DROP TABLE IF EXISTS {keyValuePair1.TableName};");

                    }
                    else
                    {

                    }


                    CustomDBQuery(keyValuePair1.BuildCreateTableSql());
                }
            }
            return true;
        }


        public void DataBaseSelectAll(string strSql, ref DataSet dataSet, ref System.Data.DataTable dataTable)
        {

            lock (DBLock)
            {
                try
                {
                    MySqlCommand mySqlCommand = new MySqlCommand(strSql, this.MyConnect);
                    MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
                    mySqlDataAdapter.Fill(dataSet, "user");
                    dataTable = dataSet.Tables["user"];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }
        public T[] Select<T>(string sql) where T : new()
        {
            var results = new List<T>();
            RunMysql(connection =>
            {
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                var reader = cmd.ExecuteReader();
                var properties = typeof(T).GetProperties();
                while (reader.Read())
                {
                    var obj = new T();

                    foreach (var property in properties)
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                        {
                            property.SetValue(obj, reader[property.Name]);
                        }
                    }

                    results.Add(obj);
                }
            });
            return results.ToArray();

        }
    }

    public class MysqlTableAbs
    {
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();

        public string TableName { get; set; }

        /// <summary>
        /// 构造创建表的数据
        /// </summary>
        /// <returns></returns>
        public string BuildCreateTableSql()
        {
            // 拼接所有列的定义
            string columnsDef = string.Join(",\n", Columns.Select(c => GetColumnDef(c)));

            // 组装 SQL 语句
            string sql = $"CREATE TABLE IF NOT EXISTS `{TableName}` (\n{columnsDef}\n) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

            return sql;
        }

        public bool ComparisonField(IEnumerable<TableColumn> columns)
        {

            return Columns.All(Cl => columns.Any(cl => cl == Cl)) && columns.All(Cl => Columns.Any(cl => cl == Cl));
        }

        private static string GetColumnDef(TableColumn column)
        {
            // 构建列的定义
            string def = $"`{column.Field}` {column.Type}";

            if (column.Null == "NO")
            {
                def += " NOT NULL";
            }

            if (!string.IsNullOrEmpty(column.Default))
            {
                def += $" DEFAULT '{column.Default}'";
            }

            if (column.Key == "PRI")
            {
                def += " PRIMARY KEY";
            }
            else if (column.Key == "UNI")
            {
                def += " UNIQUE KEY";
            }

            if (!string.IsNullOrEmpty(column.Extra))
            {
                def += $" {column.Extra}";
            }

            return def;
        }

        public string ConvertColumnsToString(IEnumerable<TableColumn> columns)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("columns = new TableColumn[]\n");
            sb.Append("{\n");
            foreach (TableColumn column in columns)
            {
                sb.Append($"    new TableColumn() {{ Field = \"{column.Field}\", Type = \"{column.Type}\", Extra = \"{column.Extra}\", Null = \"{column.Null}\", Key = \"{column.Key}\" }},\n");
            }
            sb.Append("};");
            return sb.ToString();
        }

    }

    public class TableColumn
    {
        static object lockcartFunc = new object();
        static Func<TableColumn, TableColumn, bool> operatorEqual = null;
        static Func<TableColumn, TableColumn, bool> GetOperatorEqual()
        {
            if (operatorEqual != null) return operatorEqual;
            lock (lockcartFunc)
            {
                if (operatorEqual != null) return operatorEqual;

                operatorEqual = BuildDynamicComparer<TableColumn>(true);
                return operatorEqual;
            }
        }
        public static Func<T, T, bool> BuildDynamicComparer<T>(bool ignoreCase)
        {
            // 定义动态方法的名称、返回值类型和参数类型
            string methodName = "Compare" + typeof(T).Name;
            Type returnType = typeof(bool);
            Type[] parameterTypes = new Type[] { typeof(T), typeof(T) };

            // 创建动态方法
            DynamicMethod method = new DynamicMethod(methodName, returnType, parameterTypes);

            // 获取 ILGenerator，用于生成 IL 代码
            ILGenerator il = method.GetILGenerator();

            // 定义标签用于返回 true
            Label returnTrueLabel = il.DefineLabel();

            // 获取 T 类型的所有属性
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 比较每个属性的值
            foreach (PropertyInfo prop in properties)
            {
                // 判断属性是否为字符串类型
                if (prop.PropertyType == typeof(string))
                {
                    // 将参数压入堆栈
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, prop.GetGetMethod());
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Callvirt, prop.GetGetMethod());

                    // 如果 ignoreCase 参数为 true，则调用 String.Equals(string, string, StringComparison) 方法进行比较
                    if (ignoreCase)
                    {
                        MethodInfo stringEqualsIgnoreCase = typeof(string).GetMethod("Equals", new Type[] { typeof(string), typeof(string), typeof(StringComparison) });
                        il.Emit(OpCodes.Ldc_I4, (int)StringComparison.OrdinalIgnoreCase);
                        il.Emit(OpCodes.Call, stringEqualsIgnoreCase);
                    }
                    // 否则调用 String.Equals(string, string) 方法进行比较
                    else
                    {
                        MethodInfo stringEquals = typeof(string).GetMethod("Equals", new Type[] { typeof(string), typeof(string) });
                        il.Emit(OpCodes.Call, stringEquals);
                    }

                    // 如果属性值不相等则返回 false
                    il.Emit(OpCodes.Brfalse_S, returnTrueLabel);
                }
                else
                {
                    // 将参数压入堆栈
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, prop.GetGetMethod());
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Callvirt, prop.GetGetMethod());

                    // 如果属性值不相等则返回 false
                    if (prop.PropertyType.IsValueType)
                    {
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, typeof(object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) }));
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                    }

                    il.Emit(OpCodes.Brtrue_S, returnTrueLabel);
                }
            }

            // 所有属性值都相等，返回 true
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(returnTrueLabel);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            // 创建委托并返回
            return (Func<T, T, bool>)method.CreateDelegate(typeof(Func<T, T, bool>));
        }
        /// <summary>
        /// 列名
        /// </summary>
        public string Field { get; set; } = "";
        /// <summary>
        /// 数据类型
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// 是否允许为空 (YES 或 NO)
        /// </summary>
        public string Null { get; set; } = "YES";
        /// <summary>
        /// 是否是键 (PRI 表示主键，UNI 表示唯一键，MUL 表示普通索引)
        /// </summary>
        public string Key { get; set; } = "";
        /// <summary>
        /// 默认值
        /// </summary>
        public string Default { get; set; } = "";
        /// <summary>
        /// 额外信息 (例如 auto_increment、on update CURRENT_TIMESTAMP 等)
        /// </summary>
        public string Extra { get; set; } = "";

        public static bool operator ==(TableColumn column1, TableColumn column2)
        {
            return GetOperatorEqual()(column1, column2);
        }
        public static bool operator !=(TableColumn column1, TableColumn column2)
        {
            return !GetOperatorEqual()(column1, column2);
        }
    }

}

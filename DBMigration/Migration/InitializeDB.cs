using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace DBMigration.Migration
{
    [Migration(2021013101)]
    public class InitializeDb : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("SAMPLE").WithDescription("サンプルテーブル")
                .WithColumn("ID").AsString(10).WithColumnDescription("ID")
                .WithColumn("NAME").AsString(80).WithColumnDescription("名前");
        }

        public override void Down()
        {
            Delete.Table("SAMPLE");
        }
    }
}

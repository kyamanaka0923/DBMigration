using System;
using System.Collections.Generic;
using System.Configuration;
using CommandLine;
using DBMigration.Migration;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace DBMigration
{
    class Program
    {
        [Verb("update", HelpText = "DBアップデート")]
        class UpdateOptions
        {
            [Option('n', "number", Required = false, HelpText = "アップデート バージョン")]
            public long? Version { get; set; }
        }

        [Verb("downgrade", HelpText = "DBダウングレード")]
        class DowngradeOptions
        {
            [Option('n', "number", Required = true, HelpText = "ダウングレード バージョン")]
            public long Version { get; set; }
        }

        static void Main(string[] args)
        {
            var options = Parser.Default.ParseArguments<UpdateOptions, DowngradeOptions>(args)
                .WithParsed<UpdateOptions>(UpdateRun)
                .WithParsed<DowngradeOptions>(DowngradeRun);
        }

        private static void DowngradeRun(DowngradeOptions options)
        {
            var serviceProvider = CreateServices();
            using (var scope = serviceProvider.CreateScope())
            {
                DowngradeDatabase(scope.ServiceProvider, options.Version);
            }
        }

        private static void UpdateRun(UpdateOptions options)
        {
            var serviceProvider = CreateServices();
            using (var scope = serviceProvider.CreateScope())
            {
                if (options.Version is long version)
                {
                    UpdateDatabase(scope.ServiceProvider, version);

                    return;
                }

                UpdateDatabase(scope.ServiceProvider);
            }
        }

        private static void HandleParseError(IEnumerable<Error> obj)
        {
            //throw new NotImplementedException();
        }

        private static IServiceProvider CreateServices()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddOracle12CManaged()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(InitializeDb).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider();
        }

        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        private static void UpdateDatabase(IServiceProvider serviceProvider, long version)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp(version);
        }

        private static void DowngradeDatabase(IServiceProvider serviceProvider, long version)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateDown(version);
        }
    }
}

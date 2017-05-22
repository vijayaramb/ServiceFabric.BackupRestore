﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.BackupRestore;

namespace MyStatefulService
{
	internal static class Program
	{
		/// <summary>
		/// This is the entry point of the service host process.
		/// </summary>
		private static void Main()
		{
			try
			{
				//Register the service with a FileStore.
				ServiceRuntime.RegisterServiceAsync("MyStatefulServiceType",
					context =>
					{
						string serviceName = context.ServiceName.AbsoluteUri.Replace(":", string.Empty).Replace("/", "-");
						string remoteFolderName = Path.Combine(@"c:\temp", serviceName);
                        #warning change this folder in your own project!
                        //this should not point to C:\ in production, instead use a mapped network share that stores data outside the cluster.
                        //make sure the account running this service has R/W access to the location.
                        Directory.CreateDirectory(remoteFolderName);
						var centralBackupStore = new FileStore(remoteFolderName);

						return new MyStatefulService(context, centralBackupStore, log => ServiceEventSource.Current.ServiceMessage(context, log));

					}).GetAwaiter().GetResult();

				ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(MyStatefulService).Name);

				// Prevents this host process from terminating so services keep running.
				Thread.Sleep(Timeout.Infinite);
			}
			catch (Exception e)
			{
				ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
				throw;
			}
		}
	}
}

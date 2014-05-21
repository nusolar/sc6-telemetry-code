using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Solar
{
	public interface IDataSource: IDisposable
	{
		JsonDb GetConnection();

		JsonDb GetArchive();

		void RestoreArchive(JsonDb archive);
	}

	public interface IDataServiceLayer: IDisposable
	{
		IDataSource DataSource { get; set; }

		void PushStatus(Solar.Status data);

		Task PushToDropbox();
		//int CountStatus();
		// Solar.Status GetFirstStatus();
		// bool DeleteFirstStatus();
	}

	public interface IBusinessLayer: IDisposable
	{
		IDataServiceLayer DataLayer { get; set; }

		Solar.Status Status { get; }

		void HandleUserInput(Solar.Gear gear, Solar.Signals sigs);

		Task BusinessLoop(CancellationToken token);
	}

	public interface IAppLayer
	{
		IBusinessLayer Manager { get; set; }

		Task AppLayerLoop(CancellationToken token);
	}
}

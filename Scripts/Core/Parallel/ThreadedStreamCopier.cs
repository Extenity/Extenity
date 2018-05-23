using System;
using System.IO;
using System.Threading;
using Extenity.DataToolbox;
using Timer = System.Timers.Timer;

namespace Extenity.ParallelToolbox
{

	public enum StreamCopierResult
	{
		Succeeded,
		Cancelled,
		Failed
	}

	public class ThreadedStreamCopier : IDisposable
	{
		#region Initialization

		public ThreadedStreamCopier()
			: this(TimeSpan.FromMilliseconds(500), 4096)
		{
		}

		public ThreadedStreamCopier(TimeSpan progressUpdateInterval, int bufferSize)
		{
			BufferSize = bufferSize;

			ProgressReportTimer = new Timer(progressUpdateInterval.TotalMilliseconds);
			ProgressReportTimer.Elapsed += OnProgressReportTimerElapsed;
		}

		#endregion

		#region Deinitialization

		public bool IsDisposed { get; private set; }

		public void CloseAndDisposeStreams(bool force = false)
		{
			lock (this)
			{
				if (_DestinationStream != null && (DisposeDestinationStream || force))
					StreamTools.CloseAndDisposeSafe(ref _DestinationStream);
				if (_SourceStream != null && (DisposeSourceStream || force))
					StreamTools.CloseAndDisposeSafe(ref _SourceStream);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					ProgressReportTimer.Dispose();
					OngoingWaitHandle.Close();
					CloseAndDisposeStreams();
				}
				IsDisposed = true;
			}
		}

		#endregion

		#region Process

		public bool IsOngoing { get; private set; }
		public long Position { get; private set; }
		private readonly int BufferSize;

		public void CopyAsync(Stream source, Stream destination, long expectedSize = 0, bool disposeSourceStream = true, bool disposeDestinationStream = true)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			if (IsOngoing)
			{
				if (OnFinished != null)
					OnFinished(StreamCopierResult.Cancelled, new Exception("There is an ongoing copy operation."));
				return;
			}

			IsOngoing = true;
			IsCancelled = false;
			OngoingWaitHandle.Reset();
			Position = 0;

			_SourceStream = source;
			_DestinationStream = destination;
			ExpectedSize = expectedSize;
			DisposeSourceStream = disposeSourceStream;
			DisposeDestinationStream = disposeDestinationStream;

			ThreadPool.QueueUserWorkItem(stateInfo => Process());
		}

		private void Process()
		{
			ProgressReportTimer.Start();

			Exception error = null;
			try
			{
				Copy();

				if (Result == StreamCopierResult.Succeeded)
				{
					if (OnProgressChanged != null)
						OnProgressChanged(Position);
				}
			}
			catch (Exception exception)
			{
				Result = StreamCopierResult.Failed;
				error = exception;
			}

			ProgressReportTimer.Stop();
			CloseAndDisposeStreams();
			IsOngoing = false;
			OngoingWaitHandle.Set();

			if (OnFinished != null)
				OnFinished(Result, error);
		}

		private void Copy()
		{
			var buffer = new byte[BufferSize];
			using (var binaryWriter = new BinaryWriter(DestinationStream))
			{
				int readBytes;
				while ((readBytes = SourceStream.Read(buffer, 0, buffer.Length)) != 0)
				{
					binaryWriter.Write(buffer, 0, readBytes);
					Position = DestinationStream.Position;

					if (IsCancelled)
					{
						Result = StreamCopierResult.Cancelled;
						return;
					}

					if (ExpectedSize > 0 && Position > ExpectedSize)
					{
						throw new Exception(string.Format("Stream size exceeds the expected size '{0}'.", ExpectedSize));
					}
				}

				if (ExpectedSize > 0)
				{
					if (Position != ExpectedSize)
					{
						throw new Exception(string.Format("Unexpected stream size '{0}' which should be '{1}'.", Position, ExpectedSize));
					}
				}

				Result = StreamCopierResult.Succeeded;
			}
		}

		#endregion

		#region Expected Size

		public long ExpectedSize { get; private set; }

		#endregion

		#region Streams

		private Stream _SourceStream;
		private Stream _DestinationStream;
		public Stream SourceStream { get { return _SourceStream; } }
		public Stream DestinationStream { get { return _DestinationStream; } }
		public bool DisposeSourceStream { get; private set; }
		public bool DisposeDestinationStream { get; private set; }

		#endregion

		#region Progress And Completion State

		public delegate void ProgressEvent(long bytesReceived);
		public event ProgressEvent OnProgressChanged;
		public delegate void FinishedEvent(StreamCopierResult result, Exception error);
		public event FinishedEvent OnFinished;

		private readonly Timer ProgressReportTimer;
		private long PreviousReportedProgress;
		public readonly ManualResetEvent OngoingWaitHandle = new ManualResetEvent(true);

		public StreamCopierResult Result { get; private set; }

		private void OnProgressReportTimerElapsed(object sender, EventArgs eventArgs)
		{
			if (!IsOngoing)
				return;

			var currentPosition = Position;
			if (currentPosition != PreviousReportedProgress)
			{
				PreviousReportedProgress = currentPosition;
				if (OnProgressChanged != null)
					OnProgressChanged(currentPosition);
			}
		}

		#endregion

		#region Cancel

		public bool IsCancelled { get; private set; }

		/// <summary>
		/// Cancels the copy operation. Extra attention may be needed if cancellation fails after timeout. Streams won't be closed if cancellation fails and you may need to manually close streams in that case.
		/// </summary>
		public bool Cancel(int ongoingCopyWaitTimeout = 10000)
		{
			if (IsCancelled)
				return false;
			IsCancelled = true;

			// Wait for ongoing copy to finish.
			if (!OngoingWaitHandle.WaitOne(ongoingCopyWaitTimeout))
			{
				// It's still ongoing. Just ignore.
				return false;
			}

			CloseAndDisposeStreams();
			return true;
		}

		#endregion
	}

}

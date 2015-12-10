using System.Diagnostics;

public class FPSAnalyzer
{
	private Stopwatch stopwatch;

	private long currentPeriodStartTimeMs;
	private int currentTicks;
	private int currentFPS;
	private long lastTickTimeMs;
	private RunningStandardDeviation standardDeviation;
	private int lastFrameDuration;

	public delegate void OnUpdate(FPSAnalyzer me);
	public event OnUpdate onUpdate;
	public delegate void OnTick(FPSAnalyzer me);
	public event OnTick onTick;


	public FPSAnalyzer(bool startImmediately = true)
	{
		if (startImmediately)
		{
			Reset();
		}
	}

	public void SetCustomStopwatch(Stopwatch stopwatch)
	{
		this.stopwatch = stopwatch;
	}

	public void Reset()
	{
		if (stopwatch == null)
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
		}
		currentPeriodStartTimeMs = stopwatch.ElapsedMilliseconds;
		currentTicks = 0;
		currentFPS = 0;
		lastTickTimeMs = 0;

		standardDeviation = new RunningStandardDeviation();
	}

	public void Tick()
	{
		Tick(stopwatch.ElapsedMilliseconds);
	}

	public void Tick(long elapsedMilliseconds)
	{
		lock (this)
		{
			currentTicks++;

			if (lastTickTimeMs != 0) // To ignore first tick
			{
				var diffMs = (int)(elapsedMilliseconds - lastTickTimeMs);
				standardDeviation.Push(diffMs);
				lastFrameDuration = diffMs;
			}

			lastTickTimeMs = elapsedMilliseconds;

			if (elapsedMilliseconds - currentPeriodStartTimeMs >= 1000)
			{
				currentPeriodStartTimeMs += 1000;
				currentFPS = currentTicks;
				currentTicks = 0;

				if (onUpdate != null)
				{
					onUpdate(this);
				}
			}

			if (onTick != null)
			{
				onTick(this);
			}
		}
	}

	public int FPS
	{
		get { return currentFPS; }
	}

	public double Mean
	{
		get { return standardDeviation.Mean; }
	}

	public double Variance
	{
		get { return standardDeviation.Variance; }
	}

	public double StandardDeviation
	{
		get { return standardDeviation.StandardDeviation; }
	}

	public int LastFrameDuration
	{
		get { return lastFrameDuration; }
	}

	public double LastFrameDurationDeviation
	{
		get { return (double)lastFrameDuration - Mean; }
	}

	public double LastFrameDurationDeviationOverStandardDeviation
	{
		get
		{
			var dev = StandardDeviation;
			if (dev > 0)
			{
				return (double)LastFrameDurationDeviation / dev;
			}
			return 0;
		}
	}

	public Stopwatch Stopwatch
	{
		get { return stopwatch; }
	}
}

﻿using System;

public class RunningStandardDeviation
{
	private int m_n;
	private double m_oldM, m_newM, m_oldS, m_newS;

	public RunningStandardDeviation()
	{
		m_n = 0;
	}

	public void Clear()
	{
		m_n = 0;
	}

	public void Push(double x)
	{
		m_n++;

		// See Knuth TAOCP vol 2, 3rd edition, page 232
		if (m_n == 1)
		{
			m_oldM = m_newM = x;
			m_oldS = 0.0;
		}
		else
		{
			m_newM = m_oldM + (x - m_oldM) / m_n;
			m_newS = m_oldS + (x - m_oldM) * (x - m_newM);

			// set up for next iteration
			m_oldM = m_newM;
			m_oldS = m_newS;
		}
	}

	public int NumDataValues
	{
		get { return m_n; }
	}

	public double Mean
	{
		get
		{
			return m_n > 0
					? m_newM
					: 0.0;
		}
	}

	public double Variance
	{
		get
		{
			return m_n > 1
					? m_newS / (m_n - 1)
					: 0.0;
		}
	}

	public double StandardDeviation
	{
		get { return Math.Sqrt(Variance); }
	}
}

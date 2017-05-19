using System.Collections.Generic;
using System.Linq;

namespace Extenity.StringUtilities
{

	/// <summary>
	/// A mimetic poly-alloy of the Quicksilver scoring algorithm, essentially LiquidMetal.
	/// Source: https://github.com/rmm5t/liquidmetal
	/// </summary>
	public static class LiquidMetalStringMatcher
	{
		#region Configuration

		public const float NoMatchScore = 0.0f;
		public const float MatchScore = 1.0f;
		public const float TrailingScore = 0.8f;
		public const float TrailingButStartedScore = 0.9f;
		public const float BufferScore = 0.85f;
		public const string WordSeparators = " \t_-";

		#endregion

		#region Calculate

		public static float Score(string text, string searchPattern)
		{
			List<float> scoreArray;
			return Score(text, searchPattern, out scoreArray);
		}

		public static float Score(string text, string searchPattern, out List<float> scoreArray)
		{
			scoreArray = null;

			// short circuits
			if (searchPattern.Length == 0)
				return TrailingScore;
			if (searchPattern.Length > text.Length)
				return NoMatchScore;

			// match & score all
			var allScores = new List<List<float>>();
			var search = text.ToLowerInvariant();
			searchPattern = searchPattern.ToLowerInvariant();
			ScoreAll(text, search, searchPattern, -1, 0, new List<float>(), allScores);

			// complete miss
			if (allScores.Count == 0)
				return 0f;

			// sum per-character scores into overall scores,
			// selecting the maximum score
			var maxScore = 0f;
			for (var i = 0; i < allScores.Count; i++)
			{
				var scores = allScores[i];
				var scoreSum = 0f;
				for (var j = 0; j < text.Length; j++)
				{
					scoreSum += scores[j];
				}
				if (scoreSum > maxScore)
				{
					maxScore = scoreSum;
					scoreArray = scores;
				}
			}

			// normalize max score by string length
			// s. t. the perfect match score = 1
			maxScore /= text.Length;

			return maxScore;
		}

		private static void ScoreAll(string text, string search, string searchPattern, int searchIndex, int searchPatternIndex, List<float> scores, List<List<float>> allScores)
		{
			// save completed match scores at end of search
			if (searchPatternIndex == searchPattern.Length)
			{
				// add trailing score for the remainder of the match
				var started = search[0] == searchPattern[0];
				var trailScore = started ? TrailingButStartedScore : TrailingScore;
				FillArray(scores, trailScore, scores.Count, text.Length);
				allScores.Add(scores.ToList()); // save score clone (since reference is persisted in scores)
				return;
			}

			// consume current char to match
			var c = searchPattern[searchPatternIndex];
			searchPatternIndex++;

			// cancel match if a character is missing
			var index = search.IndexOf(c, searchIndex >= 0 ? searchIndex : 0);
			if (index == -1)
				return;

			// match all instances of the abbreviaton char
			var scoreIndex = searchIndex; // score section to update
			while ((index = search.IndexOf(c, searchIndex + 1)) != -1)
			{
				// score this match according to context
				if (IsNewWord(text, index))
				{
					ExpandList(scores, index - 1);
					scores[index - 1] = 1;
					FillArray(scores, BufferScore, scoreIndex + 1, index - 1);
				}
				else if (char.IsUpper(text[index]))
				{
					FillArray(scores, BufferScore, scoreIndex + 1, index);
				}
				else
				{
					FillArray(scores, NoMatchScore, scoreIndex + 1, index);
				}

				ExpandList(scores, index);
				scores[index] = MatchScore;

				// consume matched string and continue search
				searchIndex = index;
				ScoreAll(text, search, searchPattern, searchIndex, searchPatternIndex, scores, allScores);
			}
		}

		#endregion

		#region Tools

		private static void ExpandList(List<float> list, int index)
		{
			while (list.Count - 1 < index)
				list.Add(0f);
		}

		private static bool IsNewWord(string str, int index)
		{
			index = index - 1;
			if (index < 0 || index >= str.Length)
				return false;
			var c = str[index];
			return WordSeparators.Contains(c);
		}

		private static void FillArray(List<float> array, float value, int from, int to)
		{
			ExpandList(array, to);
			for (var i = from; i < to; i++)
			{
				array[i] = value;
			}
		}

		#endregion
	}

}

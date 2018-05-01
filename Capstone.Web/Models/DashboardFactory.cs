﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capstone.Web.Models
{
    public class DashboardFactory
    {
        private const int thresholdScore = -1;
        private const float thresholdAvg = 0;

        public DashboardFactory(UserProfile profile)
        {
            _profile = profile;
        }

        private UserProfile _profile;
        
        private float GetAverageScore()
        {
            float holes = 0;
            float score = 0;
            float avg = 0;

            foreach(ScoredMatch sm in _profile.Scores)
            {
                if (sm.Holes == 9)
                {
                    ScoredMatch csm = ConvertTo18Holes(sm);
                    holes += csm.Holes;
                    score += csm.Score;
                }
                else
                {
                    holes += sm.Holes;
                    score += sm.Score;
                }
            }

            if (holes > 0)
            {
                avg = score / holes;
            }

            return avg;
        }

        private int GetBestStrokes18()
        {
            int best = -1;

            foreach (ScoredMatch sm in _profile.Scores)
            {
                if (sm.Holes==18)
                {
                    if ((best==-1) || (sm.Score < best))
                    {
                        best = sm.Score;
                    }
                }
            }

            return best;
        }

        private int GetBestStrokes9()
        {
            int best = -1;

            foreach (ScoredMatch sm in _profile.Scores)
            {
                if (sm.Holes == 9)
                {
                    if ((best == -1) || (sm.Score < best))
                    {
                        best = sm.Score;
                    }
                }
            }

            return best;

        }

        private ScoredMatch ConvertTo18Holes(ScoredMatch score)
        {
            return new ScoredMatch()
            {
                Score = score.Score * 2,
                Holes = score.Holes * 2
            };
        }

        private bool RealScore(int i)
        {
            return (i > thresholdScore);
        }
        private bool RealScore(float i)
        {
            return (i > thresholdScore);
        }

        private string MessageBest18()
        {
            string text = "No 18 hole rounds played!";
            int score = GetBestStrokes18();
            if (RealScore(score))
            {
                text = $"Best strokes in 18 holes: {score}";
            }

            return text;
        }

        private string MessageBest9()
        {
            string text = "No 9 hole rounds played!";
            int score = GetBestStrokes9();
            if (RealScore(score))
            {
                text = $"Best strokes in 9 holes: {score}";
            }

            return text;
        }

        private string MessageAverage()
        {
            string text = "No games yet!";
            float avg = GetAverageScore();
            if (RealScore(avg))
            {
                text = $"Average score: {avg}";
            }

            return text;
        }

        public DashboardStats AssembleDashboard()
        {
            return new DashboardStats()
            {
                Best18 = this.MessageBest18(),
                Best9 = this.MessageBest9(),
                Average18 = this.MessageAverage(),
                User = _profile
            };
        }

    }
}
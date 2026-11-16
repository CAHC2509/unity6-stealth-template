using System.Collections.Generic;
using UnityEngine;

namespace StealthController.Core
{
    public class MissionLogic
    {
        private readonly List<CheckpointData> checkpoints = new List<CheckpointData>();

        private Bounds exitBounds;
        private bool hasExit;
        private bool hasActiveCheckpoint;
        private bool hasReachedExit;
        private int activeCheckpointIndex;

        public bool HasActiveCheckpoint => hasActiveCheckpoint;
        public CheckpointData ActiveCheckpoint => checkpoints[activeCheckpointIndex];
        public bool HasReachedExit => hasReachedExit;
        public bool HasExit => hasExit;
        public Bounds ExitBounds => exitBounds;
        public int CheckpointCount => checkpoints.Count;
        public int ActiveCheckpointRank => hasActiveCheckpoint ? RankOf(activeCheckpointIndex) : -1;

        public void SetCheckpoints(IReadOnlyList<CheckpointData> source)
        {
            checkpoints.Clear();

            for (int i = 0; i < source.Count; i++)
                checkpoints.Add(source[i]);

            Restart();
        }

        public void SetExit(Bounds bounds)
        {
            exitBounds = bounds;
            hasExit = true;
        }

        public void Restart()
        {
            hasReachedExit = false;
            hasActiveCheckpoint = TryFindStartCheckpoint(out activeCheckpointIndex);
        }

        public bool TryActivateCheckpointByRank(int rank)
        {
            if (rank < 0 || rank >= checkpoints.Count) return false;

            for (int i = 0; i < checkpoints.Count; i++)
            {
                if (RankOf(i) != rank) continue;

                activeCheckpointIndex = i;
                hasActiveCheckpoint = true;
                hasReachedExit = false;
                return true;
            }

            return false;
        }

        public void Tick(PlayerPose pose)
        {
            if (hasReachedExit) return;

            Vector3 bodyCenter = pose.Position + Vector3.up * (pose.CapsuleHeight * 0.5f);

            UpdateActiveCheckpoint(bodyCenter);

            if (hasExit && exitBounds.Contains(bodyCenter))
                hasReachedExit = true;
        }

        private void UpdateActiveCheckpoint(Vector3 bodyCenter)
        {
            for (int i = 0; i < checkpoints.Count; i++)
            {
                if (hasActiveCheckpoint && i == activeCheckpointIndex) continue;
                if (!checkpoints[i].Bounds.Contains(bodyCenter)) continue;
                if (hasActiveCheckpoint && checkpoints[i].Order < ActiveCheckpoint.Order) continue;

                activeCheckpointIndex = i;
                hasActiveCheckpoint = true;
            }
        }

        private int RankOf(int checkpointIndex)
        {
            int rank = 0;
            CheckpointData checkpoint = checkpoints[checkpointIndex];

            for (int i = 0; i < checkpoints.Count; i++)
            {
                if (i == checkpointIndex) continue;

                CheckpointData other = checkpoints[i];

                if (other.Order < checkpoint.Order || (other.Order == checkpoint.Order && i < checkpointIndex))
                    rank++;
            }

            return rank;
        }

        private bool TryFindStartCheckpoint(out int startIndex)
        {
            startIndex = 0;

            if (checkpoints.Count == 0) return false;

            for (int i = 1; i < checkpoints.Count; i++)
            {
                if (checkpoints[i].Order < checkpoints[startIndex].Order)
                    startIndex = i;
            }

            return true;
        }
    }
}

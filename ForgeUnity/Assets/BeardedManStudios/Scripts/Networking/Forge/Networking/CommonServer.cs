using BeardedManStudios.Forge.Networking.Frame;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BeardedManStudios.Forge.Networking
{
	public class CommonServerLogic
	{
		private NetWorker server;

		public CommonServerLogic(NetWorker server)
		{
			this.server = server;
		}

		/// <summary>
		/// Check if the target <see cref="NetworkingPlayer"/> should receive the <see cref="FrameStream"/> message
		/// </summary>
		/// <param name="player">The target <see cref="NetworkingPlayer"/> to send the <see cref="FrameStream"/> message to</param>
		/// <param name="frame">The <see cref="FrameStream"/> message to send</param>
		/// <param name="proximityDistance">The proximity distance to use for checking.</param>
		/// <param name="skipPlayer">The <see cref="NetworkingPlayer"/> to skip</param>
		/// <param name="proximityModeUpdateFrequency">The frequency of proximity updates (number of updates to skip before sending one)</param>
		/// <returns>True if the target <see cref="NetworkingPlayer"/> should receive the message, false otherwise</returns>
		public bool PlayerIsReceiver(NetworkingPlayer player, FrameStream frame, float proximityDistance, NetworkingPlayer skipPlayer = null, int proximityModeUpdateFrequency = 0)
		{
			// Don't send messages to a player who has not been accepted by the server yet
			if ((!player.Accepted && !player.PendingAccepted) || player == skipPlayer)
				return false;

			if (player == frame.Sender)
			{
				// Don't send a message to the sending player if it was meant for others
				if (frame.Receivers == Receivers.Others || frame.Receivers == Receivers.OthersBuffered || frame.Receivers == Receivers.OthersProximity || frame.Receivers == Receivers.OthersProximityGrid)
					return false;
			}

			// check if sender is null as it doesn't get sent in certain cases
			if (frame.Sender != null)
			{
				return PlayerIsDistanceReceiver(frame.Sender, player, frame, proximityDistance, proximityModeUpdateFrequency);
			}
			return true;
		}

		/// <summary>
		/// Checks if target <see cref="NetworkingPlayer"/> should receive a proximity based <see cref="FrameStream"/> message
		/// </summary>
		/// <param name="sender">The sending <see cref="NetworkingPlayer"/></param>
		/// <param name="player">The target <see cref="NetworkingPlayer"/></param>
		/// <param name="frame">The <see cref="FrameStream"/> message</param>
		/// <param name="proximityDistance">The proximity distance to use for the check</param>
		/// <param name="proximityModeUpdateFrequency">The update frequency proximity messages should be sent at (number of updates to skip before sending one)</param>
		/// <returns></returns>
		public bool PlayerIsDistanceReceiver(NetworkingPlayer sender, NetworkingPlayer player, FrameStream frame, float proximityDistance, float proximityModeUpdateFrequency)
		{
			// check for distance here so the owner doesn't need to be sent in stream, used for NCW field proximity check
			if (sender != null)
			{
				if ((frame.Receivers == Receivers.AllProximity || frame.Receivers == Receivers.OthersProximity))
				{
					return ProximityDistanceCheck(sender, player, proximityDistance, proximityModeUpdateFrequency);
				}
				else if((frame.Receivers == Receivers.AllProximityGrid || frame.Receivers == Receivers.OthersProximityGrid))
				{
					return ProximityGridCheck(sender, player, proximityDistance, proximityModeUpdateFrequency);
				}
			}
			return true;
		}

		/// <summary>
		/// Check if the target player is in proximity distance to the sender.
		/// </summary>
		/// <param name="sender">The sending <see cref="NetworkingPlayer"/></param>
		/// <param name="player">The target <see cref="NetworkingPlayer"/></param>
		/// <param name="proximityDistance">The proximity distance the target needs to be from the sender to receive an update</param>
		/// <param name="proximityModeUpdateFrequency">The frequency of updates to be sent to the target if in proximity (number of updates to skip before sending one)</param>
		/// <returns>True if the target player is in proximity distance, otherwise false</returns>
		private bool ProximityDistanceCheck(NetworkingPlayer sender, NetworkingPlayer player, float proximityDistance, float proximityModeUpdateFrequency)
		{
			// If the target player is in proximity distance the send them the frame
			if (IsInProximityDistance(player.ProximityLocation, sender.ProximityLocation, proximityDistance))
				return true;

			// if update frequency is 0, it shouldn't ever get updated while too far otherwise send update based on frequency
			return proximityModeUpdateFrequency != 0 && ProximityUpdateCountCheck(sender, player, proximityModeUpdateFrequency);
		}

		private bool ProximityGridCheck(NetworkingPlayer sender, NetworkingPlayer player, float proximityDistance, float proximityModeUpdateFrequency)
		{
			// If the target player is not in the same proximity grid zone as the sender
			// then it should not be sent to that player
			if (!sender.gridPosition.IsSameOrNeighbourCell(player.gridPosition))
			{
				// if update frequency is 0, it shouldn't ever get updated while too far
				if (proximityModeUpdateFrequency == 0)
					return false;

				return ProximityUpdateCountCheck(sender, player, proximityModeUpdateFrequency);

			}
			return true;
		}

		/// <summary>
		/// Check if the target location <see cref="Vector"/> and the source <see cref="Vector"/>'s distance is less then or equal to the proximity distance
		/// </summary>
		/// <param name="target"></param>
		/// <param name="source"></param>
		/// <param name="proximityDistance"></param>
		/// <returns></returns>
		private bool IsInProximityDistance(Vector target, Vector source, float proximityDistance)
		{
			return target.DistanceSquared(source) <= proximityDistance * proximityDistance;
		}

		/// <summary>
		/// Check to see if a far away <see cref="NetworkingPlayer"/> should still receive an update.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="player"></param>
		/// <param name="proximityModeUpdateFrequency"></param>
		/// <returns></returns>
		private bool ProximityUpdateCountCheck(NetworkingPlayer sender, NetworkingPlayer player, float proximityModeUpdateFrequency)
		{
			// if player update counts are stored, increment or update and reset them, if not, store them starting with 0
			string key = player.Ip + player.NetworkId.ToString();
			if (sender.PlayersProximityUpdateCounters.ContainsKey(key))
			{
				if (sender.PlayersProximityUpdateCounters[key] < proximityModeUpdateFrequency)
				{
					// increment update counter until it reaches the limit
					sender.PlayersProximityUpdateCounters[key]++;
					return false;
				}
				else
					sender.PlayersProximityUpdateCounters[key] = 0;
			}
			else
				sender.PlayersProximityUpdateCounters.Add(key, 0);

			return true;
		}

		/// <summary>
		/// Checks all of the clients to see if any of them are timed out
		/// </summary>
		public void CheckClientTimeout(Action<NetworkingPlayer> timeoutDisconnect)
		{
			List<NetworkingPlayer> timedoutPlayers = new List<NetworkingPlayer>();
			while (server.IsBound)
			{
				server.IteratePlayers((player) =>
				{
					// Don't process the server during this check
					if (player == server.Me)
						return;

					if (player.TimedOut())
					{
						timedoutPlayers.Add(player);
					}
				});

				if (timedoutPlayers.Count > 0)
				{
					foreach (NetworkingPlayer player in timedoutPlayers)
						timeoutDisconnect(player);

					timedoutPlayers.Clear();
				}

				// Wait a second before checking again
				Thread.Sleep(1000);
			}
		}

		/// <summary>
		/// Disconnects a client
		/// </summary>
		/// <param name="client">The target client to be disconnected</param>
		public void Disconnect(NetworkingPlayer player, bool forced,
			List<NetworkingPlayer> DisconnectingPlayers, List<NetworkingPlayer> ForcedDisconnectingPlayers)
		{
			if (player.IsDisconnecting || DisconnectingPlayers.Contains(player) || ForcedDisconnectingPlayers.Contains(player))
				return;

			if (!forced)
				DisconnectingPlayers.Add(player);
			else
				ForcedDisconnectingPlayers.Add(player);
		}
	}
}

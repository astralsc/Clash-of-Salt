using ClashofClans.Database;
using ClashofClans.Files.CsvUtils;
using ClashofClans.Logic.Clan;
using ClashofClans.Logic.Manager.Items;
using ClashofClans.Logic.Sessions;
using ClashofClans.Utilities.Netty;
using ClashofClans.Utilities.Utils;
using DotNetty.Buffers;
using Newtonsoft.Json;
using System;
using System.Text;

namespace ClashofClans.Logic
{
    public class Player
    {
        public Player(long id)
        {
            Home = new Home.Home(id, GameUtils.GenerateToken);
        }

        public Player()
        {
            // Player.
        }

        public Home.Home Home { get; set; }

        [JsonIgnore] public Device Device { get; set; }

        public void RankingEntry(IByteBuffer packet)
        {
            // TODO
        }

        public void LogicClientHome(IByteBuffer packet)
        {
            packet.WriteInt(0);

            // Home Id
            packet.WriteLong(Home.Id);

            packet.WriteInt(0); // ShieldDurationSeconds
            packet.WriteInt(0); // GuardDurationSeconds
            packet.WriteInt(0); // PersonalBreakSeconds

            packet.WriteCompressedString(Home.GameObjectManager.Save());
            packet.WriteCompressedString("{\"event\":[]}");
            //packet.WriteCompressedString("{\"Village2\":{\"TownHallMaxLevel\":5}}");
            // packet.WriteCompressedString("{\"Village2\":{\"TownHallMaxLevel\":9,\"ScoreChangeForLosing\":[{\"Milestone\":0,\"Percentage\":0},{\"Milestone\":400,\"Percentage\":30},{\"Milestone\":800,\"Percentage\":55},{\"Milestone\":1200,\"Percentage\":70},{\"Milestone\":1600,\"Percentage\":85},{\"Milestone\":2000,\"Percentage\":95},{\"Milestone\":2400,\"Percentage\":100}],\"StrengthRangeForScore\":[{\"Milestone\":0,\"Percentage\":60},{\"Milestone\":200,\"Percentage\":80},{\"Milestone\":400,\"Percentage\":100},{\"Milestone\":600,\"Percentage\":120},{\"Milestone\":800,\"Percentage\":140},{\"Milestone\":1000,\"Percentage\":160},{\"Milestone\":1200,\"Percentage\":180},{\"Milestone\":1400,\"Percentage\":200},{\"Milestone\":1600,\"Percentage\":400},{\"Milestone\":1800,\"Percentage\":600},{\"Milestone\":2000,\"Percentage\":1000}]},\"KillSwitches\":{\"TestValue\":true}}");
        }

        public void LogicClientAvatar(IByteBuffer packet)
        {
            if (Home.CurrentSeasonMonth != DateTime.Now.Month)
            {
                Home.PreviousSeasonTrophies = Home.Trophies;
                Home.PreviousSeasonMonth = DateTime.Now.Month - 1;
                Home.CurrentSeasonMonth = DateTime.Now.Month;

                if (Home.Trophies >= 5000)
                    Home.Trophies = 5000;

                Home.League = 0;
                Home.AttacksWon = 0;
                Home.DefensesWon = 0;
            }

            packet.WriteLong(Home.Id); // AccountId
            packet.WriteLong(Home.Id); // HomeId

            var alliance = Home.AllianceInfo;
            packet.WriteBoolean(alliance.HasAlliance);
            if (alliance.HasAlliance)
            {
                packet.WriteLong(alliance.Id);
                packet.WriteScString(alliance.Name);
                packet.WriteInt(alliance.Badge);
                packet.WriteInt(alliance.Role);
                packet.WriteInt(alliance.Level);
            }

            packet.WriteBoolean(false);
            for (var i = 0; i < 11; i++)
                packet.WriteInt(0);
            packet.WriteInt(Home.League);

            packet.WriteInt(0); // AllianceCastleLevel
            packet.WriteInt(10); // AllianceCastleTotalCapacity
            packet.WriteInt(0); // AllianceCastleUsedCapacity
            packet.WriteInt(0);
            packet.WriteInt(-1);
            packet.WriteInt(Home.GameObjectManager.GetTownhallLevel());

            packet.WriteScString(Home.Name); // Name
            packet.WriteScString(Home.FacebookId);
            packet.WriteInt(Home.ExpLevel); // ExpLevel
            packet.WriteInt(Home.ExpPoints); // ExpPoints
            packet.WriteInt(Home.Diamonds); // Diamonds
            packet.WriteInt(Home.Diamonds); // FreeDiamonds
            packet.WriteInt(1200); // AttackRating
            packet.WriteInt(60); // AttackKFactor
            packet.WriteInt(Home.Trophies); // Trophies
            packet.WriteInt(Home.AttacksWon);
            packet.WriteInt(0); // Donated units
            packet.WriteInt(0); // Attack losses
            packet.WriteInt(0); // Received units
            packet.WriteInt(0); // Alliance gold
            packet.WriteInt(0); // Alliance elixir
            packet.WriteInt(0); // Alliance dark elixir
            packet.WriteInt(0);
            packet.WriteBoolean(true);
            packet.WriteLong(946720861000);

            packet.WriteByte((byte)(Home.NameSet == 0 ? 0 : 1));
            packet.WriteInt(0);
            packet.WriteInt(0);
            packet.WriteInt(0);
            packet.WriteInt(1); // War preference
            packet.WriteInt(0);
            packet.WriteInt(0);
            packet.WriteByte(0);

            packet.WriteInt(8); // Resource caps
            packet.WriteInt(3000001);
            packet.WriteInt(8500000);
            packet.WriteInt(3000002);
            packet.WriteInt(8500000);
            packet.WriteInt(3000003);
            packet.WriteInt(60000);
            packet.WriteInt(3000004);
            packet.WriteInt(2000000);
            packet.WriteInt(3000005);
            packet.WriteInt(2000000);
            packet.WriteInt(3000006);
            packet.WriteInt(10000);
            packet.WriteInt(3000007);
            packet.WriteInt(900000);
            packet.WriteInt(3000008);
            packet.WriteInt(900000);

            Home.Resources.Encode(packet);

            packet.WriteInt(Home.Units.Troops.Count);
            foreach (Unit troop in Home.Units.Troops)
            {
                packet.WriteInt(troop.Id);
                packet.WriteInt(troop.Count);
            }

            packet.WriteInt(Home.Units.Spells.Count);
            foreach (Unit spell in Home.Units.Spells)
            {
                packet.WriteInt(spell.Id);
                packet.WriteInt(spell.Count);
            }

            packet.WriteInt(Home.Units.Troops.Count);
            foreach (Unit troop in Home.Units.Troops)
            {
                packet.WriteInt(troop.Id);
                packet.WriteInt(troop.Level);
            }

            packet.WriteInt(Home.Units.Spells.Count);
            foreach (Unit spell in Home.Units.Spells)
            {
                packet.WriteInt(spell.Id);
                packet.WriteInt(spell.Level);
            }

            packet.WriteInt(Home.Characters.Heroes.Count);
            foreach (Hero hero in Home.Characters.Heroes)
            {
                packet.WriteInt(hero.Id);
                packet.WriteInt(hero.Level);
            }

            packet.WriteInt(Home.Characters.Heroes.Count);
            foreach (Hero hero in Home.Characters.Heroes)
            {
                packet.WriteInt(hero.Id);
                packet.WriteInt(hero.Health);
            }

            packet.WriteInt(Home.Characters.Heroes.Count);
            foreach (Hero hero in Home.Characters.Heroes)
            {
                packet.WriteInt(hero.Id);
                packet.WriteInt(hero.State);
            }

            packet.WriteInt(0); // Alliance units

            const int tutorialSteps = 35;
            packet.WriteInt(tutorialSteps);
            for (var i = 0; i < tutorialSteps; i++)
                packet.WriteInt(21000000 + i);

            packet.WriteInt(0); // Achievement IDs
            packet.WriteInt(0); // Achievement progress

            packet.WriteInt(94); // NPC levels
            for (var i = 17000000; i < 17000094; i++)
            {
                packet.WriteInt(i);
                packet.WriteInt(3);
            }

            packet.WriteInt(0); // Looted NPC gold
            packet.WriteInt(0); // Looted NPC elixir

            for (var i = 0; i < 4; i++)
                packet.WriteInt(0);

            for (var i = 0; i < 5; i++)
                packet.WriteInt(0);
        }

        public void AvatarRankingEntry(IByteBuffer packet, int order, bool isPrevious = false)
		{
			RankingEntry(packet, order, isPrevious);

			packet.WriteInt(Home.ExpLevel);
			packet.WriteInt(Home.AttacksWon); //attacks won
			packet.WriteInt(0);
			packet.WriteInt(Home.DefensesWon); //defenses won
			packet.WriteInt(0);
			packet.WriteInt(LeagueUtils.GetLeagueByScore(isPrevious ? Home.PreviousSeasonTrophies : Home.League > 0 ? Home.Trophies : 0));

			packet.WriteScString(Home.PreferredDeviceLanguage);
			packet.WriteLong(Home.Id);

			packet.WriteInt(0);
			packet.WriteInt(0);

			AllianceInfo info = Home.AllianceInfo;

			if (info.HasAlliance)
			{
				packet.WriteBoolean(true);
				{
					packet.WriteLong(info.Id);
					packet.WriteScString(info.Name);
					packet.WriteInt(info.Badge);
				}
			}
			else
			{
				packet.WriteBoolean(false);
			}
		}

		public void LeagueMemberEntry(IByteBuffer packet, int order)
		{
			packet.WriteLong(Home.Id);
			packet.WriteScString(Home.Name);
			packet.WriteInt(order);
			packet.WriteInt(Home.Trophies);
			packet.WriteInt(0);
			packet.WriteInt(0);
			packet.WriteInt(Home.AttacksWon); //attacks won
			packet.WriteInt(0);
			packet.WriteInt(Home.DefensesWon); //defenses won
			packet.WriteInt(0);

			packet.WriteLong(Home.Id);
			packet.WriteLong(Home.Id);

			AllianceInfo info = Home.AllianceInfo;

			if (info.HasAlliance)
			{
				packet.WriteBoolean(true);
				{
					packet.WriteLong(info.Id);
					packet.WriteScString(info.Name);
					packet.WriteInt(info.Badge);
				}
			}
			else
			{
				packet.WriteBoolean(false);
			}

			packet.WriteLong(Home.Id);

		}

		private void RankingEntry(IByteBuffer packet, int order, bool isPrevious = false)
		{
			packet.WriteLong(Home.Id);
			packet.WriteScString(Home.Name);

			packet.WriteInt(order);
			packet.WriteInt(isPrevious ? Home.PreviousSeasonTrophies : Home.Trophies);
			packet.WriteInt(200);
		}

		/*public async void AddEntry(AvatarStreamEntry entry)
        {
            lock (Home.Stream)
            {
                while (Home.Stream.Count >= 40)
                    Home.Stream.RemoveAt(0);

                var max = Home.Stream.Count == 0 ? 1 : Home.Stream.Max(x => x.Id);
                entry.Id = max == int.MaxValue ? 1 : max + 1; // If we ever reach that value... but who knows...

                Home.Stream.Add(entry);
            }

            await new AvatarStreamEntryMessage(Device)
            {
                Entry = entry
            }.SendAsync();
        }*/

		/// <summary>
		///     Validates this session
		/// </summary>
		public void ValidateSession()
		{
			Session session = Device.Session;
			session.Duration = (int)DateTime.UtcNow.Subtract(session.SessionStart).TotalSeconds;

			Home.TotalPlayTimeSeconds += session.Duration;

			while (Home.Sessions.Count >= 50) Home.Sessions.RemoveAt(0);

			Home.Sessions.Add(session);
		}

		public async void Save()
		{
			Home.LastSaveTime = DateTime.UtcNow;
			/*#if DEBUG
						var st = new Stopwatch();
						st.Start();

						Resources.ObjectCache.CachePlayer(this);
						await PlayerDb.SaveAsync(this);

						st.Stop();
						Logger.Log($"Player {Home.Id} saved in {st.ElapsedMilliseconds}ms.", GetType(), ErrorLevel.Debug);
			#else*/
			Resources.ObjectCache.CachePlayer(this);
			await PlayerDb.SaveAsync(this);
			//#endif
		}

		public async void SaveAll()
		{
			Home.Status = 0;
			Home.LastSaveTime = DateTime.UtcNow;
			/*#if DEBUG
                        var st = new Stopwatch();
                        st.Start();

                        Resources.ObjectCache.CachePlayer(this);
                        await PlayerDb.SaveAsync(this);

                        st.Stop();
                        Logger.Log($"Player {Home.Id} saved in {st.ElapsedMilliseconds}ms.", GetType(), ErrorLevel.Debug);
            #else*/
			Resources.ObjectCache.CachePlayer(this);
			await PlayerDb.SaveAsync(this);
			//#endif
		}
    }
}
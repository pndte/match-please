using System;
using Bw.Entities.Network;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Players.Network
{
    public sealed class ConnectedClientAdmitter
    {
        private readonly Lifetime _lifetime;
        private readonly IPlayerAdmitter _admitter;
        private readonly IPlayerDrivers _drivers;
        private readonly DepartedPlayers _departed;

        public ConnectedClientAdmitter(
            Lifetime lifetime,
            IClientCollection clients,
            IPlayerAdmitter admitter,
            IPlayerDrivers drivers,
            DepartedPlayers departed)
        {
            _lifetime = lifetime;
            _admitter = admitter;
            _drivers = drivers;
            _departed = departed;

            clients.ByIds.View(lifetime, (connectedLifetime, _, client) => Admit(connectedLifetime, client));
        }

        private void Admit(Lifetime connectedLifetime, IClient client)
        {
            var token = client.Request.Token;
            if (_departed.Contains(token))
                Return(connectedLifetime, client, _departed.PlayerOf(token));
            else
                Join(connectedLifetime, client);
        }

        private void Join(Lifetime connectedLifetime, IClient client)
        {
            var player = new Player(client.Request.Nickname);
            _admitter.Admit(_lifetime, player, client.Request.CharacterKey, PlayerDriver.Client(client));
            DepartWhenGone(connectedLifetime, player, client.Request.Token);
        }

        private void Return(Lifetime connectedLifetime, IClient client, IPlayer player)
        {
            _departed.Remove(player);
            _drivers.ChangeDriver(player, PlayerDriver.Client(client));
            DepartWhenGone(connectedLifetime, player, client.Request.Token);
        }

        private void DepartWhenGone(Lifetime connectedLifetime, IPlayer player, Guid token) =>
            connectedLifetime.OnTermination(() =>
            {
                if (!_lifetime.IsAlive)
                    return;

                _drivers.ChangeDriver(player, PlayerDriver.Server);
                _departed.Add(player, token); //TODO: на уровне без ботов (без BotsInstaller) за ушедшего никто не играет: его персонаж стоит, пока он не вернётся
            });
    }
}

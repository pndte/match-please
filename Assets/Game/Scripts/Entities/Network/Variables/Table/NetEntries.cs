using System;
using System.Collections.Generic;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetEntries : INetEntries
    {
        internal IReadOnlyList<NetEntrySlot> Slots => _slots;

        private readonly NetEntrySlot[] _slots;
        private readonly Dictionary<NetEntryDeclaration, NetEntrySlot> _byDeclaration;

        public NetEntries(NetEntriesSchema schema, INetSendGuardFactory sendGuards)
        {
            _slots = SlotsOf(schema, sendGuards);
            _byDeclaration = ByDeclaration(_slots);
        }

        public INetRequestSender<T> RequestSender<T>(NetRequestDeclaration<T> declaration) =>
            Request(declaration);

        public INetRequestReceiver<T> RequestReceiver<T>(NetRequestDeclaration<T> declaration) =>
            Request(declaration);

        public INetResultSender<T> ResultSender<T>(NetResultDeclaration<T> declaration) =>
            (NetResult<T>)SlotOf(declaration).Entry;

        public INetResultReceiver<T> ResultReceiver<T>(NetResultDeclaration<T> declaration) =>
            (NetResult<T>)SlotOf(declaration).Entry;

        public INetProperty<T> Property<T>(NetPropertyDeclaration<T> declaration) =>
            (INetProperty<T>)SlotOf(declaration).Entry;

        internal NetRequest<T> Request<T>(NetRequestDeclaration<T> declaration) =>
            (NetRequest<T>)SlotOf(declaration).Entry;

        internal NetEntrySlot At(ushort varId) =>
            varId < _slots.Length
                ? _slots[varId]
                : throw new InvalidOperationException($"No net entry with id {varId.ToString()}: the schema of this object has {_slots.Length.ToString()} entries.");

        private NetEntrySlot SlotOf(NetEntryDeclaration declaration) =>
            _byDeclaration.TryGetValue(declaration, out var slot)
                ? slot
                : throw new InvalidOperationException("The declaration is not in the schema of this object, it was declared for another net object.");

        private static NetEntrySlot[] SlotsOf(NetEntriesSchema schema, INetSendGuardFactory sendGuards)
        {
            var declarations = schema.Declarations;
            var slots = new NetEntrySlot[declarations.Count];
            for (var index = 0; index < slots.Length; index++)
            {
                var declaration = declarations[index];
                var sendGuard = sendGuards.Create(declaration.Permissions);
                slots[index] = new NetEntrySlot((ushort)index, declaration, declaration.Create(sendGuard), sendGuard);
            }

            return slots;
        }

        private static Dictionary<NetEntryDeclaration, NetEntrySlot> ByDeclaration(NetEntrySlot[] slots)
        {
            var byDeclaration = new Dictionary<NetEntryDeclaration, NetEntrySlot>(slots.Length);
            foreach (var slot in slots)
                byDeclaration.Add(slot.Declaration, slot);

            return byDeclaration;
        }
    }
}

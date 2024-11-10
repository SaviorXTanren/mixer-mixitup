using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Chat.Trovo
{
    public class TrovoChatSpellViewModel
    {
        public const string ManaValueType = "Mana";
        public const string ElixirValueType = "Elixir";

        public const string SpellNameSpecialIdentifier = "spellname";
        public const string SpellQuantitySpecialIdentifier = "spellquantity";
        public const string SpellValueSpecialIdentifier = "spellvalue";
        public const string SpellTotalValueSpecialIdentifier = "spellvaluetotal";
        public const string SpellValueTypeSpecialIdentifier = "spellvaluetype";

        public TrovoChatSpellContentModel Contents { get; set; }

        public UserV2ViewModel User { get; set; }

        public string Name { get { return this.Contents.gift; } }

        public int Quantity { get { return this.Contents.num; } }

        public int Value { get { return this.Contents.gift_value; } }

        public int ValueTotal { get { return this.Quantity * this.Value; } }

        public bool IsMana { get { return string.Equals(this.Contents.value_type, ManaValueType, StringComparison.OrdinalIgnoreCase); } }
        public bool IsElixir { get { return string.Equals(this.Contents.value_type, ElixirValueType, StringComparison.OrdinalIgnoreCase); } }
        public string ValueType
        {
            get
            {
                if (this.IsMana)
                {
                    return MixItUp.Base.Resources.TrovoMana;
                }
                else if (this.IsElixir)
                {
                    return MixItUp.Base.Resources.TrovoElixir;
                }
                return string.Empty;
            }
        }

        public TrovoChatSpellViewModel(UserV2ViewModel user, ChatMessageModel message)
        {
            this.User = user;
            this.Contents = JSONSerializerHelper.DeserializeFromString<TrovoChatSpellContentModel>(message.content);
        }

        public Dictionary<string, string> GetSpecialIdentifiers()
        {
            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
            specialIdentifiers[SpellNameSpecialIdentifier] = this.Name;
            specialIdentifiers[SpellQuantitySpecialIdentifier] = this.Quantity.ToString();
            specialIdentifiers[SpellTotalValueSpecialIdentifier] = this.ValueTotal.ToString();
            specialIdentifiers[SpellValueTypeSpecialIdentifier] = this.ValueType;
            specialIdentifiers[SpellValueSpecialIdentifier] = this.Value.ToString();
            return specialIdentifiers;
        }
    }

    public class TrovoChatSpellContentModel
    {
        public int gift_id { get; set; }
        public string gift { get; set; }
        public int num { get; set; }
        public int gift_value { get; set; }
        public string value_type { get; set; }
    }
}

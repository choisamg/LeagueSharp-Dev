﻿#region License

/*
 Copyright 2014 - 2015 Nikita Bernthaler
 Trinket.cs is part of SFXUtility.

 SFXUtility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 SFXUtility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with SFXUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion License

#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SFXLibrary.Extensions.LeagueSharp;
using SFXLibrary.Extensions.NET;
using SFXLibrary.Logger;
using SFXUtility.Classes;

#endregion

namespace SFXUtility.Features.Events
{
    internal class Trinket : Base
    {
        private const float CheckInterval = 300f;
        private float _lastCheck = Environment.TickCount;
        private Events _parent;

        public override bool Enabled
        {
            get
            {
                return !Unloaded && _parent != null && _parent.Enabled && Menu != null &&
                       Menu.Item(Name + "Enabled").GetValue<bool>();
            }
        }

        public override string Name
        {
            get { return Global.Lang.Get("F_Trinket"); }
        }

        protected override void OnEnable()
        {
            LeagueSharp.Game.OnUpdate += OnGameUpdate;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            LeagueSharp.Game.OnUpdate -= OnGameUpdate;
            base.OnDisable();
        }

        protected override void OnGameLoad(EventArgs args)
        {
            try
            {
                if (Global.IoC.IsRegistered<Events>())
                {
                    _parent = Global.IoC.Resolve<Events>();
                    if (_parent.Initialized)
                    {
                        OnParentInitialized(null, null);
                    }
                    else
                    {
                        _parent.OnInitialized += OnParentInitialized;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void OnParentInitialized(object sender, EventArgs eventArgs)
        {
            try
            {
                if (_parent.Menu == null)
                {
                    return;
                }

                Menu = new Menu(Name, Name);

                var timersMenu = new Menu(Global.Lang.Get("Trinket_Timers"), Name + "Timers");
                timersMenu.AddItem(
                    new MenuItem(timersMenu.Name + "WardingTotem", Global.Lang.Get("Trinket_WardingTotem")).SetValue(
                        new Slider(0, 0, 60)));
                timersMenu.AddItem(
                    new MenuItem(timersMenu.Name + "SweepingLens", Global.Lang.Get("Trinket_SweepingLens")).SetValue(
                        new Slider(20, 0, 60)));
                timersMenu.AddItem(
                    new MenuItem(timersMenu.Name + "ScryingOrb", Global.Lang.Get("Trinket_ScryingOrb")).SetValue(
                        new Slider(45, 0, 60)));
                timersMenu.AddItem(
                    new MenuItem(timersMenu.Name + "WardingTotemBuy", Global.Lang.Get("Trinket_WardingTotemBuy"))
                        .SetValue(false));
                timersMenu.AddItem(
                    new MenuItem(timersMenu.Name + "SweepingLensBuy", Global.Lang.Get("Trinket_SweepingLensBuy"))
                        .SetValue(false));
                timersMenu.AddItem(
                    new MenuItem(timersMenu.Name + "ScryingOrbBuy", Global.Lang.Get("Trinket_ScryingOrbBuy")).SetValue(
                        false));
                timersMenu.AddItem(
                    new MenuItem(timersMenu.Name + "Enabled", Global.Lang.Get("G_Enabled")).SetValue(false));

                var eventsMenu = new Menu(Global.Lang.Get("Trinket_Events"), Name + "Events");
                eventsMenu.AddItem(
                    new MenuItem(eventsMenu.Name + "Sightstone", Global.Lang.Get("Trinket_Sightstone")).SetValue(false));
                eventsMenu.AddItem(
                    new MenuItem(eventsMenu.Name + "RubySightstone", Global.Lang.Get("Trinket_RubySightstone")).SetValue
                        (false));
                eventsMenu.AddItem(
                    new MenuItem(eventsMenu.Name + "WrigglesLantern", Global.Lang.Get("Trinket_WrigglesLantern"))
                        .SetValue(false));

                eventsMenu.AddItem(
                    new MenuItem(eventsMenu.Name + "BuyTrinket", Global.Lang.Get("Trinket_BuyTrinket")).SetValue(
                        new StringList(
                            new[] { Global.Lang.Get("G_Yellow"), Global.Lang.Get("G_Red"), Global.Lang.Get("G_Blue") })));
                eventsMenu.AddItem(
                    new MenuItem(eventsMenu.Name + "Enabled", Global.Lang.Get("G_Enabled")).SetValue(false));

                Menu.AddSubMenu(timersMenu);
                Menu.AddSubMenu(eventsMenu);

                Menu.AddItem(
                    new MenuItem(Name + "SellUpgraded", Global.Lang.Get("Trinket_SellUpgraded")).SetValue(false));
                Menu.AddItem(new MenuItem(Name + "Enabled", Global.Lang.Get("G_Enabled")).SetValue(false));

                _parent.Menu.AddSubMenu(Menu);

                HandleEvents(_parent);
                RaiseOnInitialized();
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (_lastCheck + CheckInterval > Environment.TickCount)
                {
                    return;
                }

                _lastCheck = Environment.TickCount;

                if (ObjectManager.Player.IsDead || ObjectManager.Player.InShop())
                {
                    if (!Menu.Item(Name + "SellUpgraded").GetValue<bool>())
                    {
                        if (ObjectManager.Player.HasItem(ItemId.Greater_Vision_Totem_Trinket) ||
                            ObjectManager.Player.HasItem(ItemId.Greater_Stealth_Totem_Trinket) ||
                            ObjectManager.Player.HasItem(ItemId.Farsight_Orb_Trinket) ||
                            ObjectManager.Player.HasItem(ItemId.Oracles_Lens_Trinket))
                        {
                            return;
                        }
                    }

                    var hasYellow = ObjectManager.Player.HasItem(ItemId.Warding_Totem_Trinket) ||
                                    ObjectManager.Player.HasItem(ItemId.Greater_Vision_Totem_Trinket) ||
                                    ObjectManager.Player.HasItem(ItemId.Greater_Stealth_Totem_Trinket);
                    var hasBlue = ObjectManager.Player.HasItem(ItemId.Scrying_Orb_Trinket) ||
                                  ObjectManager.Player.HasItem(ItemId.Farsight_Orb_Trinket);
                    var hasRed = ObjectManager.Player.HasItem(ItemId.Sweeping_Lens_Trinket) ||
                                 ObjectManager.Player.HasItem(ItemId.Oracles_Lens_Trinket);

                    if (Menu.Item(Name + "EventsEnabled").GetValue<bool>())
                    {
                        bool hasTrinket;
                        var trinketId = (ItemId) 0;
                        switch (Menu.Item(Name + "EventsBuyTrinket").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                hasTrinket = hasYellow;
                                trinketId = ItemId.Warding_Totem_Trinket;
                                break;

                            case 1:
                                hasTrinket = hasRed;
                                trinketId = ItemId.Sweeping_Lens_Trinket;
                                break;

                            case 2:
                                hasTrinket = hasBlue;
                                trinketId = ItemId.Scrying_Orb_Trinket;
                                break;

                            default:
                                hasTrinket = true;
                                break;
                        }

                        if (ObjectManager.Player.HasItem(ItemId.Sightstone) &&
                            Menu.Item(Name + "EventsSightstone").GetValue<bool>())
                        {
                            if (!hasTrinket)
                            {
                                SwitchTrinket(trinketId);
                            }
                            return;
                        }
                        if (ObjectManager.Player.HasItem(ItemId.Ruby_Sightstone) &&
                            Menu.Item(Name + "EventsRubySightstone").GetValue<bool>())
                        {
                            if (!hasTrinket)
                            {
                                SwitchTrinket(trinketId);
                            }
                            return;
                        }
                        if (ObjectManager.Player.HasItem(ItemId.Wriggles_Lantern) &&
                            Menu.Item(Name + "EventsWrigglesLantern").GetValue<bool>())
                        {
                            if (!hasTrinket)
                            {
                                SwitchTrinket(trinketId);
                            }
                            return;
                        }
                    }

                    if (Menu.Item(Name + "TimersEnabled").GetValue<bool>())
                    {
                        var time = (int) (LeagueSharp.Game.Time / 60f);
                        var tsList = new List<TrinketStruct>
                        {
                            new TrinketStruct(
                                ItemId.Warding_Totem_Trinket, hasYellow,
                                Menu.Item(Name + "TimersWardingTotemBuy").GetValue<bool>(),
                                Menu.Item(Name + "TimersWardingTotem").GetValue<Slider>().Value),
                            new TrinketStruct(
                                ItemId.Sweeping_Lens_Trinket, hasRed,
                                Menu.Item(Name + "TimersSweepingLensBuy").GetValue<bool>(),
                                Menu.Item(Name + "TimersSweepingLens").GetValue<Slider>().Value),
                            new TrinketStruct(
                                ItemId.Scrying_Orb_Trinket, hasBlue,
                                Menu.Item(Name + "TimersScryingOrbBuy").GetValue<bool>(),
                                Menu.Item(Name + "TimersScryingOrb").GetValue<Slider>().Value)
                        };
                        tsList = tsList.OrderBy(ts => ts.Time).ToList();

                        for (int i = 0, l = tsList.Count; i < l; i++)
                        {
                            if (time >= tsList[i].Time)
                            {
                                var hasHigher = false;
                                if (i != l - 1)
                                {
                                    for (var j = i + 1; j < l; j++)
                                    {
                                        if (time >= tsList[j].Time && tsList[j].Buy)
                                        {
                                            hasHigher = true;
                                        }
                                    }
                                }
                                if (!hasHigher && tsList[i].Buy && !tsList[i].HasItem)
                                {
                                    SwitchTrinket(tsList[i].ItemId);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void SwitchTrinket(ItemId itemId)
        {
            try
            {
                if ((int) itemId <= 0)
                {
                    return;
                }
                var iItem =
                    ObjectManager.Player.InventoryItems.FirstOrDefault(
                        slot =>
                            slot.IsValidSlot() && slot.Name.Contains("Trinket", StringComparison.OrdinalIgnoreCase) ||
                            slot.DisplayName.Contains("Trinket", StringComparison.OrdinalIgnoreCase));
                if (iItem != null)
                {
                    ObjectManager.Player.SellItem(iItem.Slot);
                }
                ObjectManager.Player.BuyItem(itemId);
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private struct TrinketStruct
        {
            public readonly bool Buy;
            public readonly bool HasItem;
            public readonly ItemId ItemId;
            public readonly int Time;

            public TrinketStruct(ItemId itemId, bool hasItem, bool buy, int time)
            {
                ItemId = itemId;
                HasItem = hasItem;
                Buy = buy;
                Time = time;
            }
        }
    }
}
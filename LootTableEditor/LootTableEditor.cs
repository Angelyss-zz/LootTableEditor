﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;

namespace LootTableEditor
{
    [APIVersion(1,13)]
    public class LootTableEditor : TerrariaPlugin
    {
        public Config Config;

        public override string Author
        {
            get { return "Zack Piispanen"; }
        }

        public override string Description
        {
            get { return "Override vanilla npc loot tables"; }
        }

        public override string Name
        {
            get { return "NPC Loot table editor"; }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public LootTableEditor(Main game) : base(game)
        {
            Order = 1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Hooks.NpcHooks.NPCLootDrop -= OnLootDrop;
        }

        public override void Initialize()
        {
            Config = new Config();
            Config.ReadFile(Path.Combine(TShock.SavePath, "LootDrop.json"));
            Hooks.NpcHooks.NPCLootDrop += OnLootDrop;
        }

        Random random = new Random();
        private void OnLootDrop(Hooks.NpcLootDropEventArgs args)
        {
            //Debug print
            //Console.WriteLine("{0}[{1}]: ({2}, {3}) - Item:{4}", args.NPCID, args.NPCArrayIndex, args.X, args.Y,
            //      args.ItemID);

            if (Config.LootReplacements.ContainsKey(args.NPCID))
            {
                DropReplacement repl = Config.LootReplacements[args.NPCID];
                double rng = random.NextDouble();
                foreach(Drop d in repl.drops)
                {
                    if(d.chance >= rng)
                    {
                        var item = TShock.Utils.GetItemById(d.itemID);
                        int stack = random.Next(d.low_stack, d.high_stack + 1);
                        Item.NewItem(args.X, args.Y, item.width, item.height, d.itemID, stack, args.Broadcast, d.prefix);

                        args.Handled = true;

                        if (!repl.tryEachItem)
                            break;

                        //Debug print
                        //Console.WriteLine("{0} was replaced with {1} of {2}", args.ItemID, d.itemID, stack);
                    }
                }

                if (repl.alsoDropDefaultLoot)
                    args.Handled = true;
            }
        }
    }
}
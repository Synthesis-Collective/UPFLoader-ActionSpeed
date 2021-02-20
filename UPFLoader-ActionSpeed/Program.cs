using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UPFLoaderActionSpeed
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            return SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "ActionSpeedLoader.esp",
                        TargetRelease = GameRelease.SkyrimSE,
                        BlockAutomaticExit = true,
                    }
                });
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {

            /*
             TO DO:
             * check if exceeding 254 plugin load and warn
             * clean up, tighten up
             * custom binarywrite, to flag output as esl and stop the dummy Synthesis.esp being created
             */


            //create hash set to hold mods we want
            var modKeySet = new HashSet<ModKey>();


            //check if ActionSpeed.esp is in load order
            var mk = ModKey.FromNameAndExtension("ActionSpeed.esp");
            if (!state.LoadOrder.ContainsKey(mk))
            {
                //warning if not, stops patch
                System.Console.WriteLine($"Please add {mk} to your load order");
                return;
            }

            //adds ActionSpeed.esp to your loader as a master, as it is required by zEdit
            //patcher and this loader will not catch it otherwise. saves a manual click in zedit
            else modKeySet.Add(mk);


            // got it, carrying on
            System.Console.WriteLine($"{mk} found! Loading...");


            //detect npc records in mods, adds mods containing those records to set
            foreach (var npcGetter in state.LoadOrder.PriorityOrder.Npc().WinningOverrides())
            {
                var modKey = npcGetter.FormKey.ModKey;
                var linkedKeys = npcGetter.ContainedFormLinks.Select(l => l.FormKey.ModKey);

                //add keys to our hashset
                modKeySet.Add(modKey);
                modKeySet.Add(linkedKeys);
            }

            //make sure we get mods that make overrides to npc records
            foreach (var npcGetter in state.LoadOrder.PriorityOrder.Npc().WinningContextOverrides(state.LinkCache))
            {
                //add keys to our hashset
                modKeySet.Add(npcGetter.ModKey);
            }

            //removes null mod names that may have been gathered
            modKeySet.Remove(ModKey.Null);

            //list collected masters
            foreach (ModKey key in modKeySet)
            {
                System.Console.WriteLine(key);
            }

            //shows total number of mods we're going to have as masters
            System.Console.WriteLine("\n" + "\n" + $"Adding {modKeySet.Count} masters to loader.");

            //getting the Skyrim data path to dump our created HeartSeekerLoader.esp, will be co-opted by mod organizer anyhow
            if (!GameLocations.TryGetGameFolder(GameRelease.SkyrimSE, out var gamePath))
            {
                throw new ArgumentException("Game folder can not be located automatically");
            }
            var dataPath = Path.Combine(gamePath, "Data");

            //gets modkey from the load order
            var myLoadOrder = state.LoadOrder.Select(loadKey => loadKey.Key);

            //takes the set of mods we've collected and adds them as masters to the esp
            state.PatchMod.ModHeader.MasterReferences.AddRange(
                modKeySet.Select(m => new MasterReference()
                {
                    Master = m
                }));


            //special output of our esp to get around synthesis default, dummy synthesis esp still created
            state.PatchMod.WriteToBinary(
            Path.Combine(dataPath, "ActionSpeedLoader.esp"),
            new BinaryWriteParameters()
            {
                // Don't modify the content of the masters list with what records we have inside
                MastersListContent = BinaryWriteParameters.MastersListContentOption.NoCheck,

                // Order the masters to match load order
                //old load order getter config
                MastersListOrdering = new BinaryWriteParameters.MastersListOrderingByLoadOrder(myLoadOrder),
                //new mutagen 0.21.3, i've not got working yet
                //MastersListOrdering = new BinaryWriteParameters.MastersListOrderingByLoadOrder(state.LoadOrder),
                //Ignore default Synthesis.esp mod output name
                ModKey = BinaryWriteParameters.ModKeyOption.NoCheck,

            });
        }
    }
}

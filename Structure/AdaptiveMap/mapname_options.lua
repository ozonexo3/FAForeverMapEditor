options ={

{
    default = 1,
    label = "Dynamic Spawn Of Resources",
    help = "Determine which mexes & hydros should be spawned.",
    key = 'dynamic_spawn',
    pref = 'dynamic_spawn',
    values = {
        { text = "mirror slots", help = "Spawn resources for player & mirror slot (balanced resources).", key = 1, },
        { text = "used slots", help = "Only spawn resources for player on used slots (unbalanced resources).", key = 2, },
        { text = "no mirror = no resources", help = "Only spawn resources if mirror slot is also occupied by a player (not recommended, but it can make uneven matches fairer).", key = 3, },
        -- { text = "2v2 setup", help = "Don't adjust for player & spawn resources for 2v2.", key = 4, },
        -- { text = "4v4 setup", help = "Don't adjust for player & spawn resources for 4v4.", key = 5, },
        -- { text = "6v6 setup", help = "Don't adjust for player & spawn resources for 6v6.", key = 6, },
        -- { text = "8v8 setup", help = "Don't adjust for player & spawn resources for maximum player count.", key = 7, },
    },
},

{
    default = 1,
    label = "Crazyrush",
    help = "Activate different types of crazyrush* for the spawned mexes. *Building a mex on a mass point will always create new adjacent mass points to build on.",
    key = 'crazyrush_mexes',
    pref = 'crazyrush_mexes',
    values = {
        { text = "disabled", help = "No crazyrush.", key = 1, },
        -- { text = "crazyrush forward mexes", help = "Activate crazyrush only for the 4 center mexes. All other mexes will behave normally.", key = 2, },
        -- { text = "crazyrush 1 core mex", help = "Activate crazyrush & spawn only 1 core mex per active slot.", key = 3, },
        { text = "crazyrush", help = "Activate crazyrush for all spawned mexes.", key = 4, },
    },
},

{
    default = 1,
    label = "Regrowing Trees",
    help = "Regrow reclaimed/destroyed trees when other trees are nearby. Regrow is faster if more trees are close.",
    key = 'TreeRegrowSpeed',
    pref = 'TreeRegrowSpeed',
    values = {
        { text = "disabled", help = "No regrowing trees.", key = 1, },
        { text = "fast", help = "Regrow trees faster.", key = 2, },
        { text = "enabled", help = "Regrow trees.", key = 3, },
        { text = "slow", help = "Regrow trees slower.", key = 4, },
    },
},

{
    default = 5,
    label = "Natural Reclaim Values",
    help = "Change mass & energy values of rock & tree props.",
    key = 'naturalReclaimModifier',
    pref = 'naturalReclaimModifier',
    values = {
        --not defined via adaptive script, just determine value via key
        { text = "300 percent", help = "Mass & energy values are 3 times higher.", key = 3, },
        { text = "200 percent", help = "Mass & energy values are 2 times higher.", key = 2, },
        { text = "150 percent", help = "Mass & energy values are 1.5 times higher.", key = 1.5, },
        { text = "125 percent", help = "Mass & energy values are 1.25 times higher.", key = 1.25, },
        { text = "100 percent", help = "Don't change the mass & energy values.", key = 1, },
        { text = "75 percent", help = "Mass & energy values are 0.75 times lower.", key = 0.75, },
        { text = "50 percent", help = "Mass & energy values are 0.5 times lower.", key = 0.5, },
        { text = "25 percent", help = "Mass & energy values are 0.25 times lower.", key = 0.25, },
        { text = "0 percent", help = "Remove Mass & energy values from rock & tree props.", key = 0, },
    },
},

--[[
{
    default = 1,
    label = "Extra Hydros",
    help = "Spawn additional hydros -WHERE-.",
    key = 'extra_hydros',
    pref = 'extra_hydros',
    values = {
        { text = "0", help = "No extra hydros.", key = 1, },
        { text = "2", help = "Spawn 2 hydros.", key = 2, },
    },
},

{
    default = 1,
    label = "Extra Mexes",
    help = "Spawn additional mexes -WHERE-.",
    key = 'extra_mexes',
    pref = 'extra_mexes',
    values = {
        { text = "0", help = "No extra mexes.", key = 1, },
        { text = "4", help = "Add 4 extra mexes.", key = 2, },
    },
},

{
    default = 1,
    label = "Middle Mexes",
    help = "Configure the amount of mexes at the middle of the map.",
    key = 'middle_mexes',
    pref = 'middle_mexes',
    values = {
        { text = "0", help = "No additional mexes in the middle.", key = 1, }
        { text = "2", help = "Spawn 2 mexes in the middle.", key = 2, },
        { text = "4", help = "Spawn 4 mexes in the middle.", key = 3, },
        { text = "6", help = "Spawn 6 mexes in the middle.", key = 4, },
    },
},

{
    default = 1,
    label = "Side Mexes",
    help = "Configure the amount of mexes at the sides of the map.",
    key = 'side_mexes',
    pref = 'side_mexes',
    values = {
        { text = "0", help = "No additional mexes at the side.", key = 1, },
        { text = "2", help = "Spawn 2 mexes, 1 on each side.", key = 2, },
        { text = "4", help = "Spawn 4 mexes, 2 on each side.", key = 3, },
        { text = "6", help = "Spawn 6 mexes, 3 on each side.", key = 4, },

    },
},

{
    default = 1,
    label = "Underwater Mexes",
    help = "Configure the amount of underwater mexes.",
    key = 'underwater_mexes',
    pref = 'underwater_mexes',
    values = {
        { text = "0", help = "No additional underwater mexes.", key = 1, },
        { text = "2", help = "Spawn 2 mexes in the water around the islands.", key = 2, },
        { text = "4", help = "Spawn 4 mexes in the water around the islands.", key = 3, },
        { text = "6", help = "Spawn 6 mexes in the water around the islands.", key = 4, },
    },
},

{
    default = 1,
    label = "Island Mexes",
    help = "Configure the amount of mexes at the island.",
    key = 'island_mexes',
    pref = 'island_mexes',
    values = {
        { text = "0", help = "No additional mexes at the island.", key = 1, },
        { text = "2", help = "Spawn 2 mexes at the island.", key = 2, },
        { text = "4", help = "Spawn 4 mexes at the island.", key = 3, },
        { text = "6", help = "Spawn 6 mexes at the island.", key = 4, },
    },
},

{
    default = 1,
    label = "Expansion Mexes",
    help = "Configure the amount of mexes at the expansion.",
    key = 'expansion_mexes',
    pref = 'expansion_mexes',
    values = {
        { text = "0", help = "No additional expansion mexes.", key = 1, },
        { text = "2", help = "Spawn 2 mexes in the expansion.", key = 2, },
        { text = "4", help = "Spawn 4 mexes in the expansion.", key = 3, },
        { text = "6", help = "Spawn 6 mexes in the expansion.", key = 4, },
    },
},

{
    default = 1,
    label = "Core Mexes",
    help = "Spawn 3 or 4 core mexes.",
    key = 'core_mexes',
    pref = 'core_mexes',
    values = {
        { text = "3", help = "Spawn 3 core mexes.", key = 1, },
        { text = "4", help = "Spawn 4 core mexes.", key = 2, },
    },
},

{
    default = 1,
    label = "Extra Base Mexes",
    help = "Spawns additional mexes for each player in the starting base (further away from core mexes).",
    key = 'extra_base_mexes',
    pref = 'extra_base_mexes',
    values = {
        { text = "0", help = "No extra base mexes.", key = 1, },
        { text = "1", help = "Spawns 1 additional mex per player.", key = 2, },
    },
},

{
    default = 1,
    label = "Wreckage",
    help = "Scale amount of unit wrecks.",
    key = 'optional_wreckage',
    pref = 'optional_wreckage',
    values = {
        { text = "disabled", help = "No land/air wrecks.", key = 1, },
        { text = "T1 wrecks", help = "Add T1 wrecks.", key = 2, },
        { text = "T2 wrecks", help = "Add T2 wrecks to T1.", key = 3, },
        { text = "T3 wrecks", help = "Add T3 wrecks to T1 & T2.", key = 4, },
        { text = "T4 wreck", help = "Add Fatboy wreck to T1, T2 & T3.", key = 5, },
    },
},

{
    default = 1,
    label = "Naval Wreckage",
    help = "Scale amount of naval unit wrecks.",
    key = 'optional_naval_wreckage',
    pref = 'optional_naval_wreckage',
    values = {
        { text = "disabled", help = "No naval wrecks.", key = 1, },
        { text = "T1 wrecks", help = "Add 8 T1 Frigate wrecks.", key = 2, },
        { text = "T2 wrecks", help = "Add 8 T2 Destroyer wrecks to T1.", key = 3, },
        { text = "T3 wrecks", help = "Add 2 T3 Battleship wrecks to T1 & T2.", key = 4, },
    },
},

{
    default = 1,
    label = "Middle Wreckage",
    help = "Scale amount of unit wrecks in the middle of the map.",
    key = 'optional_wreckage_middle',
    pref = 'optional_wreckage_middle',
    values = {
        { text = "no wrecks", help = "No additional wrecks to the middle.", key = 1, },
        { text = "some wrecks", help = "Add a some T2 units to the middle of the map.", key = 2, },
        { text = "enabled", help = "Add a some T2 units and salem class destroyers to the middle of the map.", key = 3, },
        { text = "more wrecks", help = "Add a some T2 units, salem class destroyers and some T3 units to the middle of the map.", key = 4, },
        { text = "vanilla wrecks", help = "Add a some T2 units, T3 units, salem class destroyers and two experimentals to the middle of the map (like it is on the vanilla version of the map).", key = 5, },
        { text = "a lot of wrecks", help = "Add a some T2 units, T3 units, salem class destroyers and six experimentals to the middle of the map (like it is on the vanilla version of the map).", key = 6, },
    },
},

{
    default = 1,
    label = "Faction Wreckage",
    help = "Add T2 pgen wrecks to the rear air position of the map. The faction of these pgens is either UEF or Cybran depending on the player.",
    key = 'optional_adaptive_faction_wreckage',
    pref = 'optional_adaptive_faction_wreckage',
    values = {
        { text = "disabled", help = "No additional faction wreckage.", key = 1, },
        { text = "1x T2 Power", help = "Add one T2 pgen wreck with the same faction as the air player to the back position.", key = 2, },
        { text = "2x T2 Power", help = "Add two T2 pgen wreck with the same faction as the air player to the back position.", key = 3, },
    },
},

{
    default = 1,
    label = "Civilian Base",
    help = "Spawn civilian base in the middle of the map.",
    key = 'optional_civilian_base',
    pref = 'optional_civilian_base',
    values = {
        { text = "disabled", help = "No civilian base.", key = 1, },
        { text = "wreckage", help = "Spawn civilian base wreckage.", key = 2, },
        { text = "operational", help = "Spawn operational civilian base.", key = 3, },
    },
},

{
    default = 1,
    label = "Civilian Defenses",
    help = "Spawn civilian defenses at the middle plateau.",
    key = 'optional_civilian_defenses',
    pref = 'optional_civilian_defenses',
    values = {
        { text = "disabled", help = "No civilian defenses.", key = 1, },
        { text = "T1 wrecks (PD+AA)", help = "Spawn civilian T1 PD & AA wrecks. Includes T1 Radar, T1 Power Generator & Energy Storage.", key = 2, },
        { text = "T2 wrecks (PD+TMD)", help = "Spawn civilian T2 PD & TMD wrecks in addition to T1.", key = 3, },
        { text = "T3 wrecks (PD+AA)", help = "Spawn civilian T3 PD & AA wrecks in addition to T1 & T2.", key = 4, },
        { text = "T1 operational (PD+AA)", help = "Spawn operational civilian T1 PD & AA. Includes T1 Radar, T1 Power Generator & Energy Storage.", key = 5, },
        { text = "T2 operational (PD+TMD)", help = "Spawn operational civilian T2 PD & TMD in addition to T1.", key = 6, },
        { text = "T3 operational (PD+AA)", help = "Spawn operational civilian T3 PD & AA in addition to T1 & T2.", key = 7, },
    },
},

{
    default = 1,
    label = "Jamming",
    help = "Add a Seraphim jamming crystal to the map center, to create false radar signals.",
    key = 'jamming',
    pref = 'jamming',
    values = {
        { text = "disabled", help = "No jamming.", key = 1, },
        { text = "enabled", help = "Add a jamming crystal.", key = 2, },
    },
},

{
    default = 2,
    label = "Expand Map Area",
    help = "Determines how long the playable area is restricted to the area without islands. This option is takes only affect when no island spots are taken (spots 13,14).",
    key = 'expand_map',
    pref = 'expand_map',
    values = {
        { text = "no expansion", help = "Map stays restricted to the middle region.", key = 1, },
        { text = "start expanded", help = "Map starts fully expanded.", key = 2, },
        { text = "5 min", help = "Expansion after 5 minutes.", key = 3, },
        { text = "10 min", help = "Expansion after 10 minutes.", key = 4, },
        { text = "15 min", help = "Expansion after 15 minutes.", key = 5, },
        { text = "20 min", help = "Expansion after 20 minutes.", key = 6, },
        { text = "80 percent of mexes", help = "Expansion when 80 percent of the mexes are build.", key = 7, },
        { text = "90 percent of mexes", help = "Expansion when 90 percent of the mexes are build.", key = 8, },
    },
},

{
    default = 1,
    label = "Top Side Mexes",
    help = "Adjust the number of mexes in the middle of the side spots on the top side",
    key = 'top_side_mexes',
    pref = 'top_side_mexes',
    values = {
        { text = "0", help = "No additional mexes on the top side.", key = 1, },
        { text = "2", help = "2 mex on the top side.", key = 2, },
        { text = "4", help = "4 mex on the top side.", key = 3, },
        { text = "6", help = "6 mex on the top side.", key = 4, },
    },
},

{
    default = 1,
    label = "Bottom Side Mexes",
    help = "Adjust the number of mexes in the middle of the side spots on the bottom side",
    key = 'bottom_side_mexes',
    pref = 'bottom_side_mexes',
    values = {
        { text = "0", help = "No additional mexes on the bottom side.", key = 1, },
        { text = "2", help = "2 mex on the bottom side.", key = 2, },
        { text = "4", help = "4 mex on the bottom side.", key = 3, },
        { text = "6", help = "6 mex on the bottom side.", key = 4, },
    },
},

]]--

};

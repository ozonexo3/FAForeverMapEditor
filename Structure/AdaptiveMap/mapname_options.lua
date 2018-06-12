-- This file adds options to the game lobby. There are two types of options in this file
-- 1) options that will automatically work when you fill out the adaptive map tables for the 
--    players. These options are enabled by default. Some sub-options require also additional tables
--    they are deactivated by default, but you can easily activate them by removing the "--" before the
--    deactivated option.
-- 2) options that require an additional table to work (or other things). These options start
--    a bit further down in the file and if you want to use them, just move them above the comment 
--    section. Dont forget to add the required tables. 

-- IMPORTANT: If you change the default for the option, make sure to also adjust the script! The default
--            key values of the options are stored at the top of the script file. For example:
--            "local dynamic_spawn = ScenarioInfo.Options.dynamic_spawn or 1"
--            If you want the default to be something different, then you also have to change it here.
--            Also keep in mind to use the "key" of the selected option, which is not necessarily the
--            same as "default" of the option (the default value specifies the number of the option that
--            is used as the default).

-- The options are now using the language that is set in the game for the names of the options and
-- the explanation text. Most options that spawn a specific number of mexes have translations prepared
-- for up to 9 mex. Just use for example "adaptive_middle_mex_key_7_help" when you want to have the
-- text for 7 mex. Keep in mind that you also have to adjust the label, but NOT the key = XX of the option.

-- If you do not want to use the standard text and write something different, just remove the 
-- "<LOC adaptive_somethingsomething> the standard text you dont want" part and write your own text.
-- Obviously this text wont translate and will always show english.

options ={

{
    default = 1,
    label = "<LOC adaptive_dynamic_spawn_label> Dynamic Spawn Of Resources",
    help = "<LOC adaptive_dynamic_spawn_help> Determine which mexes & hydros should be spawned.",
    key = 'dynamic_spawn',
    pref = 'dynamic_spawn',
    values = {
        { text = "<LOC adaptive_dynamic_spawn_key_1_label> mirror slots", help = "<LOC adaptive_dynamic_spawn_key_1_help> Spawn resources for player & mirror slot (balanced resources).", key = 1, },
        { text = "<LOC adaptive_dynamic_spawn_key_2_label> used slots", help = "<LOC adaptive_dynamic_spawn_key_2_help> Only spawn resources for player on used slots (unbalanced resources).", key = 2, },
        { text = "<LOC adaptive_dynamic_spawn_key_3_label> no mirror = no resources", help = "<LOC adaptive_dynamic_spawn_key_3_help> Only spawn resources if mirror slot is also occupied by a player (not recommended, but it can make uneven matches fairer).", key = 3, },
        -- { text = "<LOC adaptive_dynamic_spawn_key_4_label> 2v2 setup", help = "<LOC adaptive_dynamic_spawn_key_4_help> Don't adjust for player & spawn resources for 2v2.", key = 4, },
        -- { text = "<LOC adaptive_dynamic_spawn_key_5_label> 4v4 setup", help = "<LOC adaptive_dynamic_spawn_key_5_help> Don't adjust for player & spawn resources for 4v4.", key = 5, },
        -- { text = "<LOC adaptive_dynamic_spawn_key_6_label> 6v6 setup", help = "<LOC adaptive_dynamic_spawn_key_6_help> Don't adjust for player & spawn resources for 6v6.", key = 6, },
        -- { text = "<LOC adaptive_dynamic_spawn_key_7_label> 8v8 setup", help = "<LOC adaptive_dynamic_spawn_key_7_help> Don't adjust for player & spawn resources for maximum player count.", key = 7, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_crazyrush_label> Crazyrush",
    help = "<LOC adaptive_crazyrush_help> Activate different types of crazyrush* for the spawned mexes. *Building a mex on a mass point will always create new adjacent mass points to build on.",
    key = 'crazyrush_mexes',
    pref = 'crazyrush_mexes',
    values = {
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_crazyrush_key_1_help> No crazyrush.", key = 1, },
        -- { text = "<LOC adaptive_crazyrush_key_2_label> crazyrush forward mexes", help = "<LOC adaptive_crazyrush_key_2_help> Activate crazyrush only for some central mexes. All other mexes will behave normally.", key = 2, },
        -- { text = "<LOC adaptive_crazyrush_key_3_label> crazyrush 1 core mex", help = "<LOC adaptive_crazyrush_key_3_help> Activate crazyrush & spawn only 1 core mex per active slot.", key = 3, },
        { text = "<LOC adaptive_crazyrush_key_4_label> crazyrush", help = "<LOC adaptive_crazyrush_key_4_help> Activate crazyrush for all spawned mexes.", key = 4, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_regrowing_label> Regrowing Trees",
    help = "<LOC adaptive_regrowing_help> Regrow reclaimed/destroyed trees when other trees are nearby. Regrow is faster if more trees are close.",
    key = 'TreeRegrowSpeed',
    pref = 'TreeRegrowSpeed',
    values = {
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_regrowing_key_1_help> No regrowing trees.", key = 1, },
        { text = "<LOC adaptive_regrowing_key_2_label> fast", help = "<LOC adaptive_regrowing_key_2_help> Regrow trees faster.", key = 2, },
        { text = "<LOC adaptive_enabled> enabled", help = "<LOC adaptive_regrowing_key_3_help> Regrow trees.", key = 3, },
        { text = "<LOC adaptive_regrowing_key_4_label> slow", help = "<LOC adaptive_regrowing_key_4_help> Regrow trees slower.", key = 4, },
    },
},

{
    default = 5,
    label = "<LOC adaptive_natural_modifier_label> Natural Reclaim Values",
    help = "<LOC adaptive_natural_modifier_help> Change mass & energy values of rock & tree props.",
    key = 'naturalReclaimModifier',
    pref = 'naturalReclaimModifier',
    values = {
        --not defined via adaptive script, just determine value via key
        { text = "<LOC adaptive_natural_modifier_key_300_label> 300 percent", help = "<LOC adaptive_natural_modifier_key_300_help> Mass & energy values are 3 times higher.", key = 3, },
        { text = "<LOC adaptive_natural_modifier_key_200_label> 200 percent", help = "<LOC adaptive_natural_modifier_key_200_help> Mass & energy values are 2 times higher.", key = 2, },
        { text = "<LOC adaptive_natural_modifier_key_150_label> 150 percent", help = "<LOC adaptive_natural_modifier_key_150_help> Mass & energy values are 1.5 times higher.", key = 1.5, },
        { text = "<LOC adaptive_natural_modifier_key_125_label> 125 percent", help = "<LOC adaptive_natural_modifier_key_125_help> Mass & energy values are 1.25 times higher.", key = 1.25, },
        { text = "<LOC adaptive_natural_modifier_key_100_label> 100 percent", help = "<LOC adaptive_natural_modifier_key_100_help> Don't change the mass & energy values.", key = 1, },
        { text = "<LOC adaptive_natural_modifier_key_75_label> 75 percent", help = "<LOC adaptive_natural_modifier_key_75_help> Mass & energy values are 0.75 times lower.", key = 0.75, },
        { text = "<LOC adaptive_natural_modifier_key_50_label> 50 percent", help = "<LOC adaptive_natural_modifier_key_50_help> Mass & energy values are 0.5 times lower.", key = 0.5, },
        { text = "<LOC adaptive_natural_modifier_key_25_label> 25 percent", help = "<LOC adaptive_natural_modifier_key_25_help> Mass & energy values are 0.25 times lower.", key = 0.25, },
        { text = "<LOC adaptive_natural_modifier_key_0_label> 0 percent", help = "<LOC adaptive_natural_modifier_key_0_help> Remove Mass & energy values from rock & tree props.", key = 0, },
    },
},



--[[ 
-- options in this section require an additional table (or other things). Move them above this line
-- to use them.

{
    default = 1,
    label = "<LOC adaptive_hydros_label> Extra Hydros",
	-- the "help" line doesnt have a translation available
    help = "Spawn additional hydros -WHERE-.",
    key = 'extra_hydros',
    pref = 'extra_hydros',
    values = {
        { text = "0", help = "<LOC adaptive_hydros_key_1_help> No extra hydros.", key = 1, },
		-- except for the first option there are no translations for this option
        { text = "2", help = "Spawn 2 hydros.", key = 2, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_extra_mex_label> Extra Mexes",
	-- the "help" line doesnt have a translation available
    help = "Spawn additional mexes -WHERE-.",
    key = 'extra_mexes',
    pref = 'extra_mexes',
    values = {
		-- there are translations prepared for up to 9 mex.
        { text = "0", help = "<LOC adaptive_extra_mex_key_0_help> No extra mexes.", key = 1, },
        { text = "4", help = "<LOC adaptive_extra_mex_key_4_help> Spawn 4 mexes.", key = 2, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_middle_mex_label> Middle Mexes",
    help = "<LOC adaptive_middle_mex_help> Configure the amount of mexes at the middle of the map.",
    key = 'middle_mexes',
    pref = 'middle_mexes',
    values = {
		-- there are translations prepared for up to 9 mex.
        { text = "0", help = "<LOC adaptive_middle_mex_key_0_help> No additional mexes in the middle.", key = 1, }
        { text = "2", help = "<LOC adaptive_middle_mex_key_2_help> Spawn 2 mexes in the middle.", key = 2, },
        { text = "4", help = "<LOC adaptive_middle_mex_key_4_help> Spawn 4 mexes in the middle.", key = 3, },
        { text = "6", help = "<LOC adaptive_middle_mex_key_6_help> Spawn 6 mexes in the middle.", key = 4, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_side_mex_label> Side Mexes",
    help = "<LOC adaptive_side_mex_help> Configure the amount of mexes at the sides of the map.",
    key = 'side_mexes',
    pref = 'side_mexes',
    values = {
		-- there are translations prepared for up to 9 mex per side, so 18 mex in total.
        { text = "0", help = "<LOC adaptive_side_mex_key_0_help> No additional mexes at the side.", key = 1, },
        { text = "1", help = "<LOC adaptive_side_mex_key_1_help> Spawn 2 mexes, 1 on each side.", key = 2, },
        { text = "2", help = "<LOC adaptive_side_mex_key_2_help> Spawn 4 mexes, 2 on each side.", key = 3, },
        { text = "3", help = "<LOC adaptive_side_mex_key_3_help> Spawn 6 mexes, 3 on each side.", key = 4, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_underwater_mex_label> Underwater Mexes",
    help = "<LOC adaptive_underwater_mex_help> Configure the amount of underwater mexes.",
    key = 'underwater_mexes',
    pref = 'underwater_mexes',
    values = {
		-- there are translations prepared for up to 9 mex.
        { text = "0", help = "<LOC adaptive_underwater_mex_key_0> No additional underwater mexes.", key = 1, },
        { text = "2", help = "<LOC adaptive_underwater_mex_key_2> Spawn 2 mexes in the water around the islands.", key = 2, },
        { text = "4", help = "<LOC adaptive_underwater_mex_key_4> Spawn 4 mexes in the water around the islands.", key = 3, },
        { text = "6", help = "<LOC adaptive_underwater_mex_key_6> Spawn 6 mexes in the water around the islands.", key = 4, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_island_mex_label> Island Mexes",
    help = "<LOC adaptive_island_mex_help> Configure the amount of mexes at the island.",
    key = 'island_mexes',
    pref = 'island_mexes',
    values = {
		-- there are translations prepared for up to 9 mex.
        { text = "0", help = "<LOC adaptive_island_mex_key_0> No additional mexes at the island.", key = 1, },
        { text = "2", help = "<LOC adaptive_island_mex_key_2> Spawn 2 mexes at the island.", key = 2, },
        { text = "4", help = "<LOC adaptive_island_mex_key_4> Spawn 4 mexes at the island.", key = 3, },
        { text = "6", help = "<LOC adaptive_island_mex_key_6> Spawn 6 mexes at the island.", key = 4, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_expansion_mex_label> Expansion Mexes",
    help = "<LOC adaptive_expansion_mex_help> Configure the amount of mexes at the expansion.",
    key = 'expansion_mexes',
    pref = 'expansion_mexes',
    values = {
		-- there are translations prepared for up to 9 mex.
        { text = "0", help = "<LOC adaptive_expansion_mex_key_0> No additional expansion mexes.", key = 1, },
        { text = "2", help = "<LOC adaptive_expansion_mex_key_2> Spawn 2 mexes in the expansion.", key = 2, },
        { text = "4", help = "<LOC adaptive_expansion_mex_key_4> Spawn 4 mexes in the expansion.", key = 3, },
        { text = "6", help = "<LOC adaptive_expansion_mex_key_6> Spawn 6 mexes in the expansion.", key = 4, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_core_mex_label> Core Mexes",
    help = "<LOC adaptive_core_mex_2_help> Spawn 4 or 5 core mexes.",
    key = 'core_mexes',
    pref = 'core_mexes',
    values = {
		-- there are no additional translations for this option
        { text = "4", help = "<LOC adaptive_core_mex_key_4> Spawn 4 core mexes.", key = 1, },
        { text = "5", help = "<LOC adaptive_core_mex_key_5> Spawn 5 core mexes.", key = 2, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_extra_base_mex_label> Extra Base Mexes",
    help = "<LOC adaptive_extra_base_mex_help> Spawns additional mexes for each player in the starting base (further away from core mexes).",
    key = 'extra_base_mexes',
    pref = 'extra_base_mexes',
    values = {
		-- there are translations prepared for up to 5 mex.
        { text = "0", help = "<LOC adaptive_extra_base_mex_key_0> No extra base mexes.", key = 1, },
        { text = "1", help = "<LOC adaptive_extra_base_mex_key_1> Spawns 1 additional mex per player.", key = 2, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_wreckage_label> Wreckage",
    help = "<LOC adaptive_wreckage_help> Scale amount of unit wrecks.",
    key = 'optional_wreckage',
    pref = 'optional_wreckage',
    values = {
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_wreckage_key_1_help> No land/air wrecks.", key = 1, },
        { text = "<LOC adaptive_wreckage_key_2> T1 wrecks", help = "<LOC adaptive_wreckage_key_2_help> Add T1 wrecks.", key = 2, },
        { text = "<LOC adaptive_wreckage_key_3> T2 wrecks", help = "<LOC adaptive_wreckage_key_3_help> Add T2 wrecks to T1.", key = 3, },
        { text = "<LOC adaptive_wreckage_key_4> T3 wrecks", help = "<LOC adaptive_wreckage_key_4_help> Add T3 wrecks to T1 & T2.", key = 4, },
        { text = "<LOC adaptive_wreckage_key_5> T4 wreck", help = "<LOC adaptive_wreckage_key_5_help> Add Fatboy wreck to T1, T2 & T3.", key = 5, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_naval_wreckage_label> Naval Wreckage",
    help = "<LOC adaptive_naval_wreckage_help> Scale amount of naval unit wrecks.",
    key = 'optional_naval_wreckage',
    pref = 'optional_naval_wreckage',
    values = {
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_naval_wreckage_key_1_help> No naval wrecks.", key = 1, },
        { text = "<LOC adaptive_naval_wreckage_key_2> T1 wrecks", help = "<LOC adaptive_naval_wreckage_key_2_help> Add several T1 Frigate wrecks.", key = 2, },
        { text = "<LOC adaptive_naval_wreckage_key_3> T2 wrecks", help = "<LOC adaptive_naval_wreckage_key_3_help>Add some T2 navy wrecks in addition to the T1 wreckage.", key = 3, },
        { text = "<LOC adaptive_naval_wreckage_key_4> T3 wrecks", help = "<LOC adaptive_naval_wreckage_key_4_help>Add some T3 ship wrecks in addition to the T1 & T2 wreckage.", key = 4, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_middle_wreckage_label> Middle Wreckage",
    help = "<LOC adaptive_middle_wreckage_help> Scale amount of unit wrecks in the middle of the map.",
    key = 'optional_wreckage_middle',
    pref = 'optional_wreckage_middle',
    values = {
		-- the help text is partly not localized (i.e. it wont translate to the different languages) since it is impossible to predict what you will use
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_middle_wreckage_key_0_help> No additional wrecks to the middle.", key = 1, },
        { text = "<LOC adaptive_middle_wreckage_key_1> few wrecks", help = "Add a some T2 units to the middle of the map.", key = 2, },
        { text = "<LOC adaptive_middle_wreckage_key_2> some", help = "Add a some T2 units and salem class destroyers to the middle of the map.", key = 3, },
        { text = "<LOC adaptive_middle_wreckage_key_3> more wrecks", help = "Add a some T2 units, salem class destroyers and some T3 units to the middle of the map.", key = 4, },
        { text = "<LOC adaptive_middle_wreckage_key_4> a lot of wrecks", help = "Add a some T2 units, T3 units, salem class destroyers and six experimentals to the middle of the map (like it is on the vanilla version of the map).", key = 5, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_factional_wreckage_label> Faction Wreckage",
    help = "<LOC adaptive_factional_wreckage_help> Add some building wrecks to the map. The faction of these wrecks is the same as the adjacent player.",
    key = 'optional_adaptive_faction_wreckage',
    pref = 'optional_adaptive_faction_wreckage',
    values = {
		-- the help text is partly not localized (i.e. it wont translate to the different languages) since it is impossible to predict what you will use
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_factional_wreckage_key_0_help> No additional faction wreckage.", key = 1, },
        { text = "1x T2 Power", help = "Add one T2 pgen wreck with the same faction as the air player to the back position.", key = 2, },
        { text = "2x T2 Power", help = "Add two T2 pgen wreck with the same faction as the air player to the back position.", key = 3, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_civ_base_label> Civilian Base",
    help = "<LOC adaptive_civ_base_help> Spawn civilian base in the middle of the map.",
    key = 'optional_civilian_base',
    pref = 'optional_civilian_base',
    values = {
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_civ_base_key_1_help> No civilian base.", key = 1, },
        { text = "<LOC adaptive_civ_base_key_2> wreckage", help = "<LOC adaptive_civ_base_key_2_help> Spawn civilian base wreckage.", key = 2, },
        { text = "<LOC adaptive_civ_base_key_3> operational", help = "<LOC adaptive_civ_base_key_3_help> Spawn operational civilian base.", key = 3, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_civ_def_label> Civilian Defenses",
    help = "<LOC adaptive_civ_def_label> Spawn civilian defenses at the middle plateau.",
    key = 'optional_civilian_defenses',
    pref = 'optional_civilian_defenses',
    values = {
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_civ_def_key_0_help> No civilian defenses.", key = 1, },
        { text = "<LOC adaptive_civ_def_key_1> T1 wrecks (PD+AA)", help = "<LOC adaptive_civ_def_key_1_help> Spawn civilian T1 PD & AA wrecks. Includes T1 Radar, T1 Power Generator & Energy Storage.", key = 2, },
        { text = "<LOC adaptive_civ_def_key_2> T2 wrecks (PD+TMD)", help = "<LOC adaptive_civ_def_key_2_help> Spawn civilian T2 PD & TMD wrecks in addition to T1.", key = 3, },
        { text = "<LOC adaptive_civ_def_key_3> T3 wrecks (PD+AA)", help = "<LOC adaptive_civ_def_key_3_help> Spawn civilian T3 PD & AA wrecks in addition to T1 & T2.", key = 4, },
        { text = "<LOC adaptive_civ_def_key_4> T1 operational (PD+AA)", help = "<LOC adaptive_civ_def_key_4_help> Spawn operational civilian T1 PD & AA. Includes T1 Radar, T1 Power Generator & Energy Storage.", key = 5, },
        { text = "<LOC adaptive_civ_def_key_5> T2 operational (PD+TMD)", help = "<LOC adaptive_civ_def_key_5_help> Spawn operational civilian T2 PD & TMD in addition to T1.", key = 6, },
        { text = "<LOC adaptive_civ_def_key_6> T3 operational (PD+AA)", help = "<LOC adaptive_civ_def_key_6_help> Spawn operational civilian T3 PD & AA in addition to T1 & T2.", key = 7, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_jamming_label> Jamming",
    help = "<LOC adaptive_jamming_help> Add a Seraphim jamming crystal to the map center, to create false radar signals.",
    key = 'jamming',
    pref = 'jamming',
    values = {
        { text = "<LOC adaptive_disabled> disabled", help = "<LOC adaptive_jamming_key_1> No jamming.", key = 1, },
        { text = "<LOC adaptive_enabled> enabled", help = "<LOC adaptive_jamming_key_2> Add a jamming crystal.", key = 2, },
    },
},

{
    default = 2,
    label = "<LOC adaptive_expansion_area_label> Expand Map Area",
    help = "<LOC adaptive_expansion_area_help> Determines how long the playable area is restricted to the area without islands. This option is takes only affect when no island spots are taken (spots 13,14).",
    key = 'expand_map',
    pref = 'expand_map',
    values = {
        { text = "<LOC adaptive_expansion_area_key_1> no expansion", help = "<LOC adaptive_expansion_area_key_1_help> Map stays restricted to the middle region.", key = 1, },
        { text = "<LOC adaptive_expansion_area_key_2> start expanded", help = "<LOC adaptive_expansion_area_key_2_help> Map starts fully expanded.", key = 2, },
        { text = "<LOC adaptive_expansion_area_key_3> 5 min", help = "<LOC adaptive_expansion_area_key_3_help> Expansion after 5 minutes.", key = 3, },
        { text = "<LOC adaptive_expansion_area_key_4> 10 min", help = "<LOC adaptive_expansion_area_key_4_help> Expansion after 10 minutes.", key = 4, },
        { text = "<LOC adaptive_expansion_area_key_5> 15 min", help = "<LOC adaptive_expansion_area_key_5_help> Expansion after 15 minutes.", key = 5, },
        { text = "<LOC adaptive_expansion_area_key_6> 20 min", help = "<LOC adaptive_expansion_area_key_6_help> Expansion after 20 minutes.", key = 6, },
        { text = "<LOC adaptive_expansion_area_key_7> 80 percent of mexes", help = "<LOC adaptive_expansion_area_key_7_help> Expansion when 80 percent of the mexes are build.", key = 7, },
        { text = "<LOC adaptive_expansion_area_key_8> 90 percent of mexes", help = "<LOC adaptive_expansion_area_key_8_help> Expansion when 90 percent of the mexes are build.", key = 8, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_top_mex_label> Top Side Mexes",
    help = "<LOC adaptive_top_mex_help> Adjust the number of mexes in the middle of the side spots on the top side",
    key = 'top_side_mexes',
    pref = 'top_side_mexes',
    values = {
		-- there are translations prepared for up to 9 mex.
        { text = "0", help = "<LOC adaptive_top_mex_key_0> No additional mexes on the top side.", key = 1, },
        { text = "2", help = "<LOC adaptive_top_mex_key_2> 2 mex on the top side.", key = 2, },
        { text = "4", help = "<LOC adaptive_top_mex_key_4> 4 mex on the top side.", key = 3, },
        { text = "6", help = "<LOC adaptive_top_mex_key_6> 6 mex on the top side.", key = 4, },
    },
},

{
    default = 1,
    label = "<LOC adaptive_bottom_mex_label> Bottom Side Mexes",
    help = "<LOC adaptive_bottom_mex_help> Adjust the number of mexes in the middle of the side spots on the bottom side",
    key = 'bottom_side_mexes',
    pref = 'bottom_side_mexes',
    values = {
		-- there are translations prepared for up to 9 mex.
        { text = "0", help = "<LOC adaptive_bottom_mex_key_0> No additional mexes on the bottom side.", key = 1, },
        { text = "2", help = "<LOC adaptive_bottom_mex_key_2> 2 mex on the bottom side.", key = 2, },
        { text = "4", help = "<LOC adaptive_bottom_mex_key_4> 4 mex on the bottom side.", key = 3, },
        { text = "6", help = "<LOC adaptive_bottom_mex_key_6> 6 mex on the bottom side.", key = 4, },
    },
},
]]--

};

options ={

{
    default = 1,
    label = "dynamic spawn of mexes",
    help = "Enable the mex spawn algorithm to adjust the map to the player count",
    key = 'automex',
    pref = 'automex',
    values = {
        { text = "enabled", help = "map adjust automatically for the playercount", key = 1, },
        { text = "disabled - 4v4 mex", help = "Map will not adjust for players and will spawn the mexes of the four usual players and their expansions. Use the spots 1-8", key = 2, },
        { text = "disabled - all mex", help = "Map will not adjust for players and will spawn the all mexes", key = 3, },
        { text = "enabled - crazyrush 1 mex", help = "Map will activate the crazyrush script and spawn 1 starting mex for all players.", key = 6, },
        { text = "enabled - crazyrush", help = "Map will activate the crazyrush script.", key = 7, },
    },
},

{
    default = 1,
    label = "regrowing trees",
    help = "Regrow reclaimed trees slowly when there are still other trees around. If there are more trees close, they regrow faster.",
    key = 'tree',
    pref = 'tree',
    values = {
        { text = "disabled", help = "Dont regrow trees.", key = 1, },
        { text = "enabled - faster", help = "Regrow trees faster.", key = 2, },
        { text = "enabled", help = "Regrow trees.", key = 3, },
        { text = "enabled - slower", help = "Regrow trees slower.", key = 4, },
    },
},

{
    default = 5,
    label = "change natural reclaim",
    help = "Changes the amount of mass and energy in rocks and trees on the map.",
    key = 'naturalReclaimModifier',
    pref = 'naturalReclaimModifier',
    values = {
        { text = "3 times higher", help = "Enhances the reclaim by a factor of 3.", key = 3, },
        { text = "2 times higher", help = "Enhances the reclaim by a factor of 2.", key = 2, },
        { text = "1.5 times higher", help = "Enhances the reclaim by a factor of 1.5.", key = 1.5, },
        { text = "1.25 times higher", help = "Enhances the reclaim by a factor of 1.25.", key = 1.25, },
        { text = "disabled", help = "Doesnt change the reclaim values.", key = 1, },
        { text = "0.8 times lower", help = "Removes 20 percent of the mass in rocks.", key = 0.8, },
        { text = "0.6 times lower", help = "Removes 40 percent of the mass in rocks.", key = 0.6, },
        { text = "0.4 times lower", help = "Removes 60 percent the mass in rocks.", key = 0.4, },
        { text = "0.2 times lower", help = "Removes 80 percent the mass in rocks.", key = 0.2, },
    },
},


{
    default = 1,
    label = "mirror spots",
    help = "Only spawn ressources when mirror spots are taken",
    key = 'mirrormex',
    pref = 'mirrormex',
    values = {
        { text = "disabled", help = "Spawn mex for players that dont have a mirror. The empty mirror spot will also spawn its mex.", key = 1, },
        { text = "enabled", help = "Only spawn mex/hydro when opposing spot is also taken. This option is not recommended, however it can make uneven matches fairer.", key = 2, },

    },
},





--[[
{
    default = 1,
    label = "additional hydros",
    help = "additional hydros in the oceans.",
    key = 'hydroplus',
    pref = 'hydroplus',
    values = {
        { text = "disabled", help = "Only spawn a hydro for the players.", key = 1, },
        { text = "enabled", help = "Also spawn a hydro on the bottom of the ocean.", key = 2, },
    },
},

{
    default = 1,
    label = "forward crazyrush mex",
    help = "makes a few mexes behave like crazyrush mexes.",
    key = 'dupicatesinglemex',
    pref = 'dupicatesinglemex',
    values = {
        { text = "disabled", help = "All mexes behave normally.", key = 1, },
        { text = "enabled", help = "Some expansion mex split into multiple mexes.", key = 2, },
    },
},

{
    default = 1,
    label = "number middlemex",
    help = "configure the amount of mexes in the middle region of the map.",
    key = 'middlemex',
    pref = 'middlemex',
    values = {
        { text = "enabled - more mex", help = "Spawn 4 additional mexes in the middle.", key = 1, },
        { text = "enabled", help = "Spawn 2 additional mexes in the middle.", key = 2, },
        { text = "disabled", help = "Spawn no additional mexes in the middle.", key = 3, }
    },
},

{
    default = 3,
    label = "number sidemex",
    help = "configure the amount of mexes on the sides of the map (behind the rock position).",
    key = 'sidemex',
    pref = 'sidemex',
    values = {
        { text = "enabled - more mex", help = "Spawn 2 additional mexes on each side near the small lake.", key = 1, },
        { text = "enabled", help = "Spawn 1 additional mexes on each side near the small lake.", key = 2, },
        { text = "disabled", help = "Spawn no additional mexes on the side.", key = 3, },
    },
},

{
    default = 1,
    label = "number islandmex",
    help = "configure the amount of mexes on the islands of the map.",
    key = 'islandmex',
    pref = 'islandmex',
    values = {
        { text = "enabled - 5 mex", help = "Spawn 5 mexes on the island.", key = 1, },
        { text = "enabled - 4 mex", help = "Spawn 4 mexes on the island.", key = 2, },
        { text = "enabled - 3 mex", help = "Spawn 3 mexes on the island.", key = 3, },
        { text = "enabled - 2 mex", help = "Spawn 2 mexes on the island.", key = 4, },
        { text = "enabled - 1 mex", help = "Spawn 1 mexes on the island.", key = 5, },
    },
},

{
    default = 1,
    label = "airplayer expansion mex",
    help = "configure the amount of mexes on the expansion of the air player.",
    key = 'backmex',
    pref = 'backmex',
    values = {
        { text = "enabled - 3 mex", help = "Spawn 3 mexes in the central region of the landmass between the front and air position.", key = 1, },
        { text = "enabled - 2 mex", help = "Spawn 2 mexes in the central region of the landmass between the front and air position.", key = 2, },
        { text = "enabled - 1 mex", help = "Spawn 1 mex in the central region of the landmass between the front and air position.", key = 3, },
        { text = "disabled", help = "Spawn no mexes in the central region of the landmass between the front and air position.", key = 4, },
    },
},

{
    default = 5,
    label = "additional underwatermex",
    help = "additional sidemex in both oceans.",
    key = 'underwatermex',
    pref = 'underwatermex',
    values = {
        { text = "enabled - much more mex", help = "Spawn 11 additional mexes in the water around the islands.", key = 1, },
        { text = "enabled - more mex", help = "Spawn 8 additional mexes in the water around the islands.", key = 2, },
        { text = "enabled", help = "Spawn 5 additional mexes in the water around the islands.", key = 3, },
        { text = "enabled - fewer mex", help = "Spawn 2 additional mexes in the water around the islands.", key = 4, },
        { text = "disabled", help = "Spawn no additional mexes in the water around the islands.", key = 5, },
    },
},

{
    default = 2,
    label = "expand map area",
    help = "Determines how long the playable area is restricted to the area without islands. This option is takes only affect when no island spots are taken (spots 13,14).",
    key = 'expandmap',
    pref = 'expandmap',
    values = {
        { text = "no expansion", help = "Map stays restricted to the middle region.", key = 1, },
        { text = "start expanded", help = "Map starts fully expanded.", key = 2, },
        { text = "5 min", help = "Expansion after 5 minutes.", key = 3, },
        { text = "10 min", help = "Expansion after 10 minutes.", key = 4, },
        { text = "15 min", help = "Expansion after 5 minutes.", key = 5, },
        { text = "20 min", help = "Expansion after 10 minutes.", key = 6, },
        { text = "80 percent of mexes", help = "Expansion when 80 percent of the mexes are build.", key = 7, },
        { text = "90 percent of mexes", help = "Expansion when 90 percent of the mexes are build.", key = 8, },
    },
},

{
    default = 4,
    label = "reclaim - middle",
    help = "add wrecks to the middle of the map",
    key = 'optional_reclaim_middle',
    pref = 'optional_reclaim_middle',
    values = {
        { text = "no reclaim", help = "Do not add additional reclaim to the middle.", key = 1, },
        { text = "some reclaim", help = "Add a some t2 units to the middle of the map.", key = 2, },
        { text = "enabled", help = "Add a some t2 units and salem class destroyers to the middle of the map.", key = 3, },
        { text = "more reclaim", help = "Add a some t2 units, salem class destroyers and some t3 units to the middle of the map.", key = 4, },
        { text = "vanilla reclaim", help = "Add a some t2 units, t3 units, salem class destroyers and two experimentals to the middle of the map (like it is on the vanilla version of the map).", key = 5, },
        { text = "a lot of reclaim", help = "Add a some t2 units, t3 units, salem class destroyers and six experimentals to the middle of the map (like it is on the vanilla version of the map).", key = 6, },
    },
},

{
    default = 1,
    label = "reclaim - back",
    help = "add t2 pgen wrecks to the air rear air position of the map. The faction of these pgens is either UEF or Cybran depending on the player.",
    key = 'optional_reclaim_back',
    pref = 'optional_reclaim_back',
    values = {
        { text = "no reclaim", help = "Do not add additional reclaim to the back position.", key = 1, },
        { text = "one t2 pgen", help = "Add one t2 pgen wreck with the same faction as the air player to the back position.", key = 2, },
        { text = "two t2 pgens", help = "Add two t2 pgen wreck with the same faction as the air player to the back position.", key = 3, },
    },
},

{
    default = 1,
    label = "Civilian Base",
    help = "Spawn a civilian base in the middle.",
    key = 'optional_civilian_base',
    pref = 'optional_civilian_base',
    values = {        
        { text = "disabled", help = "Spawn no civilians on the side islands.", key = 1, },
        { text = "enabled - t1 defences", help = "Spawn a small civilian base with some t1 point defences on the side islands.", key = 2, },
        { text = "enabled - t2 defences", help = "Spawn a small civilian base with some t2 point defences on the side islands.", key = 3, },        
        { text = "enabled - arty/t3 defences", help = "Spawn a small civilian base with some t3 sams and some t2 artys on the side islands.", key = 4, },
    },
},

{
    default = 1,
    label = "additional reclaim",
    help = "add groups of wrecks to the map",
    key = 'optional_reclaim',
    pref = 'optional_reclaim',
    values = {
        { text = "no reclaim", help = "Do not add additional reclaim in the middle of the map", key = 1, },
        { text = "some reclaim", help = "Add some frigates to the beaches of the map.", key = 2, },
        { text = "more reclaim", help = "Add some destroyers to the map.", key = 3, },
        { text = "destro+frigates", help = "Add some frigates and destroyers to the map.", key = 4, },
    },
},

{
    default = 1,
    label = "reduce rock reclaim",
    help = "Reduces the amount of mass in rocks on the map on game launch.",
    key = 'removeRock',
    pref = 'removeRock',
    values = {
        { text = "disabled", help = "Rocks have the full mass value.", key = 1, },
        { text = "20 percent", help = "Removes 20 percent of the mass in rocks.", key = 2, },
        { text = "40 percent", help = "Removes 40 percent of the mass in rocks.", key = 3, },
        { text = "60 percent", help = "Removes 60 percent the mass in rocks.", key = 4, },
        { text = "80 percent", help = "Removes 80 percent the mass in rocks.", key = 5, },
    },
},

{
    default = 1,
    label = "additional mex",
    help = "Spawns one additional mex for each present player in the starting base.",
    key = 'additionalmex',
    pref = 'additionalmex',
    values = {
        { text = "disabled", help = "Spawn the usual ressources.", key = 1, },
        { text = "enabled", help = "Spawns one additional mex per player.", key = 2, },
    },
},

]]--

};
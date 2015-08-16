version = 3
ScenarioInfo = {
    name = 'Open Palms',
    description = "<LOC SCMP_007_Description>A small strip of green surrounded by ice and snow, no one is really sure who first called this area Open Palms. Legend has it that originally this was neutral ground, where soldiers of any faction could come without fear of attack. Hence, all who came had 'open palms,' meaning no weapons were carried.",
    type = 'skirmish',
    starts = true,
    preview = '',
    size = {512, 512},
    norushradius = 55,
    norushoffsetX_ARMY_1 = 30,
    norushoffsetY_ARMY_1 = 15,
    norushoffsetX_ARMY_2 = -30,
    norushoffsetY_ARMY_2 = -17,
    norushoffsetX_ARMY_3 = 2,
    norushoffsetY_ARMY_3 = 17,
    norushoffsetX_ARMY_4 = 12,
    norushoffsetY_ARMY_4 = -12,
    norushoffsetX_ARMY_5 = 5,
    norushoffsetY_ARMY_5 = 10,
    norushoffsetX_ARMY_6 = -2,
    norushoffsetY_ARMY_6 = -5,
    map = '/maps/SCMP_007/SCMP_007.scmap',
    save = '/maps/SCMP_007/SCMP_007_save.lua',
    script = '/maps/SCMP_007/SCMP_007_script.lua',
    Configurations = {
        ['standard'] = {
            teams = {
                { name = 'FFA', armies = {'ARMY_1','ARMY_2','ARMY_3','ARMY_4','ARMY_5','ARMY_6',} },
            },
            customprops = {
            },
        },
    }}

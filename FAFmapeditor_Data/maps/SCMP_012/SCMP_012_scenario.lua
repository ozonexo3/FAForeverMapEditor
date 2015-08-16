version = 3
ScenarioInfo = {
    name = 'Theta Passage',
    description = '<LOC SCMP_012_Description>In the early days of the settlement, wayfarers knew to go through the arch and head due north to reach civilization. These days, the Passage is much more dangerous, thanks to the constant fighting that moves back and forth across the desert.',
    type = 'skirmish',
    starts = true,
    preview = '',
    size = {256, 256},
    norushradius = 50,
    norushoffsetX_ARMY_1 = 15,
    norushoffsetY_ARMY_1 = -15,
    norushoffsetX_ARMY_2 = -15,
    norushoffsetY_ARMY_2 = 15,
    map = '/maps/SCMP_012/SCMP_012.scmap',
    save = '/maps/SCMP_012/SCMP_012_save.lua',
    script = '/maps/SCMP_012/SCMP_012_script.lua',
    Configurations = {
        ['standard'] = {
            teams = {
                { name = 'FFA', armies = {'ARMY_1','ARMY_2',} },
            },
            customprops = {
                ['ExtraArmies'] = STRING( 'ARMY_9 NEUTRAL_CIVILIAN' ),
            },
        },
    }}

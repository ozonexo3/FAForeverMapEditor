version = 3ScenarioInfo = {    name = 'Theta Passage',
    description = '<LOC SCMP_012_Description>In the early days of the settlement, wayfarers knew to go through the arch and head due north to reach civilization. These days, the Passage is much more dangerous, thanks to the constant fighting that moves back and forth across the desert.',
    type = 'skirmish',    starts = true,    size = {256, 256},
    map = '/maps/SCMP_012/SCMP_012.scmap',
    save = '/maps/SCMP_012/SCMP_012_save.lua',
    script = '/maps/SCMP_012/SCMP_012_script.lua',
    preview = '',
    norushradius = 50.000000,
    norushoffsetX_ARMY_1 = 15.000000,
    norushoffsetY_ARMY_1 = -15.000000,
    norushoffsetX_ARMY_2 = -15.000000,
    norushoffsetY_ARMY_2 = 15.000000,
    Configurations = {        ['standard'] = {            teams = {                { 					name = 'FFA', 					armies = {'ARMY_1','ARMY_2',}
				},            },            customprops = {            },        },    }}
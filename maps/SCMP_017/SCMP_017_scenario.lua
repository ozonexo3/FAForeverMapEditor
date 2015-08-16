version = 3
ScenarioInfo = {
    name = 'Syrtis Major',
    description = '<LOC SCMP_017_Description>This place is unremarkable in every respect. However, it does sit on a major Gate hub, offering easy access to a good portion of the galaxy. For this reason alone, many Commanders have fought to the death to claim this place.',
    type = 'skirmish',
    starts = true,
    preview = '',
    size = {512, 512},
    norushradius = 80,
    norushoffsetX_ARMY_1 = -45,
    norushoffsetY_ARMY_1 = -5,
    norushoffsetX_ARMY_2 = 45,
    norushoffsetY_ARMY_2 = 0,
    norushoffsetX_ARMY_3 = 15,
    norushoffsetY_ARMY_3 = 40,
    norushoffsetX_ARMY_4 = 0,
    norushoffsetY_ARMY_4 = -30,
    map = '/maps/SCMP_017/SCMP_017.scmap',
    save = '/maps/SCMP_017/SCMP_017_save.lua',
    script = '/maps/SCMP_017/SCMP_017_script.lua',
    Configurations = {
        ['standard'] = {
            teams = {
                { name = 'FFA', armies = {'ARMY_1','ARMY_2','ARMY_3','ARMY_4',} },
            },
            customprops = {
                ['ExtraArmies'] = STRING( 'ARMY_9 NEUTRAL_CIVILIAN' ),
            },
        },
    }}

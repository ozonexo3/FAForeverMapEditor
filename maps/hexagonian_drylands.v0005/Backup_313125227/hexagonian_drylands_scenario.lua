version = 3
ScenarioInfo = {
    name = 'Hexagonian Drylands',
    description = '<LOC hexagonian_drylands.v0005_Description>This is an ALPHA version. Released for playtesting. (MAP BY LIONHARDT) (Version 0.5)',
    type = 'skirmish',
    starts = true,
    preview = '',
    size = {512, 512},
    map = '/maps/hexagonian_drylands.v0005/hexagonian_drylands.scmap',
    save = '/maps/hexagonian_drylands.v0005/hexagonian_drylands_save.lua',
    script = '/maps/hexagonian_drylands.v0005/hexagonian_drylands_script.lua',
    norushradius = 100.000000,
    norushoffsetX_ARMY_1 = -42.000000,
    norushoffsetY_ARMY_1 = -5.000000,
    norushoffsetX_ARMY_2 = 42.000000,
    norushoffsetY_ARMY_2 = 5.000000,
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

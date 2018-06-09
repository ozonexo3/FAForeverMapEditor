-- enter maximum possible player count (army slots) on the map
maxPlayerOnMap = 16






-- table of which resources belong to which player, it is sorted in such a way that the first line corresponds to ARMY_1, the second to ARMY_2 and so on...
-- line number is 10 + armyumber for the mexes in the table
spwnMexArmy = {     {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {}}



-- line number is 30 + armyumber for the hydros in the table
spwnHydroArmy ={    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {},
                    {}}


-- RESOURCE SCALING OPTIONS
-- exampleMexes = {{1,2},{3,4},{5,6}},        -- exampleMexes = {{1,2}}
-- exampleMexes = {{a},{b},{c}},              -- exampleMexes = {{a}}
    -- option key=1 : removes a+b+c               -- option key=1 : removes a
    -- option key=2 : spawn a, removes b+c        -- option key=2 : spawns a
    -- option key=3 : spawn a+b, removes c
    -- option key=4 : spawn a+b+c

-- add extra hydros to the map
extraHydros = {{}}

-- add extra mexes to the map
extraMexes = {{}}

-- configure the amount of mexes at a certain map position
middleMexes = {{},{},{}}
sideMexes = {{},{},{}}
underwaterMexes = {{},{},{}}
islandMexes ={{},{},{}}
expansionMexes = {{},{},{}}


-- BASE RESOURCE SCALING OPTIONS
-- add core mexes 
-- dont forget to add these mexes also to spwnMexArmy
coreMexes = {{}}

-- add mexes to starting base (further away from coreMexes) 
-- dont forget to add these mexes also to spwnMexArmy
extraBaseMexes = {{}}


-- INTENTIONAL UNEVEN RESOURCE SCALING OPTIONS
topSideMexes = {{},{},{}}
bottomSideMexes = {{},{},{}}


-- CRAZYRUSH OPTIONS
-- determine forward crazy rush mexes
forwardCrazyrushMexes = {}

-- table for the "crazyrush 1 core mex" option.
-- dont forget to add these mexes also to spwnMexArmy
crazyrushOneMexes = {}

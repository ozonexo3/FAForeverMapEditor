-----------------------------------------------------------------------------------
-- BunnyRoanoke v11 Custom Map Script 
-- Author: Duck_42
-- Date: 2014.04.19
-----------------------------------------------------------------------------------

local ScenarioUtils = import('/lua/sim/ScenarioUtilities.lua')
local ScenarioFramework = import('/lua/ScenarioFramework.lua')

local Factory_Locations = {
	{X = 516.500000, Y = 889.500000},
	{X = 512.500000, Y = 135.500000},
	{X = 853.500000, Y = 702.500000},
	{X = 184.500000, Y = 320.500000},
	{X = 842.500000, Y = 324.500000},
	{X = 183.500000, Y = 699.500000}
	
}

local PGen_Locations = {
	{
		{X = 521.500000, Y = 886.500000},
		{X = 521.500000, Y = 888.500000},
		{X = 521.500000, Y = 890.500000},
		{X = 521.500000, Y = 892.500000},
		{X = 519.500000, Y = 894.500000},
		{X = 517.500000, Y = 894.500000},
		{X = 515.500000, Y = 894.500000},
		{X = 513.500000, Y = 894.500000},
		{X = 511.500000, Y = 892.500000},
		{X = 511.500000, Y = 890.500000},
		{X = 511.500000, Y = 888.500000},
		{X = 511.500000, Y = 886.500000},
	},
	{
		{X = 509.500000, Y = 130.500000},
		{X = 511.500000, Y = 130.500000},
		{X = 513.500000, Y = 130.500000},
		{X = 515.500000, Y = 130.500000},
		{X = 507.500000, Y = 132.500000},
		{X = 507.500000, Y = 134.500000},
		{X = 507.500000, Y = 136.500000},
		{X = 507.500000, Y = 138.500000},
		{X = 517.500000, Y = 132.500000},
		{X = 517.500000, Y = 134.500000},
		{X = 517.500000, Y = 136.500000},
		{X = 517.500000, Y = 138.500000},
	},
	{
		{X = 854.500000, Y = 697.500000},
		{X = 856.500000, Y = 697.500000},
		{X = 858.500000, Y = 699.500000},
		{X = 858.500000, Y = 701.500000},
		{X = 858.500000, Y = 703.500000},
		{X = 858.500000, Y = 705.500000},
		{X = 856.500000, Y = 707.500000},
		{X = 854.500000, Y = 707.500000},
		{X = 852.500000, Y = 707.500000},
		{X = 850.500000, Y = 707.500000},
		{X = 848.500000, Y = 705.500000},
		{X = 848.500000, Y = 703.500000},
	},
	{
		{X = 183.500000, Y = 325.500000},
		{X = 181.500000, Y = 325.500000},
		{X = 179.500000, Y = 323.500000},
		{X = 179.500000, Y = 317.500000},
		{X = 179.500000, Y = 319.500000},
		{X = 179.500000, Y = 321.500000},
		{X = 181.500000, Y = 315.500000},
		{X = 183.500000, Y = 315.500000},
		{X = 185.500000, Y = 315.500000},
		{X = 187.500000, Y = 315.500000},
		{X = 189.500000, Y = 317.500000},
		{X = 189.500000, Y = 319.500000},
	},
	{
		{X = 847.500000, Y = 321.500000},
		{X = 847.500000, Y = 323.500000},
		{X = 847.500000, Y = 325.500000},
		{X = 847.500000, Y = 327.500000},
		{X = 845.500000, Y = 329.500000},
		{X = 843.500000, Y = 329.500000},
		{X = 845.500000, Y = 319.500000},
		{X = 843.500000, Y = 319.500000},
		{X = 841.500000, Y = 319.500000},
		{X = 839.500000, Y = 319.500000},
		{X = 837.500000, Y = 321.500000},
		{X = 837.500000, Y = 323.500000},
	},
	{
		{X = 182.500000, Y = 694.500000},
		{X = 180.500000, Y = 694.500000},
		{X = 178.500000, Y = 696.500000},
		{X = 178.500000, Y = 698.500000},
		{X = 178.500000, Y = 700.500000},
		{X = 178.500000, Y = 702.500000},
		{X = 180.500000, Y = 704.500000},
		{X = 182.500000, Y = 704.500000},
		{X = 184.500000, Y = 704.500000},
		{X = 186.500000, Y = 704.500000},
		{X = 188.500000, Y = 702.500000},
		{X = 188.500000, Y = 700.500000},
	}
}

local EStore_T1_Locations = {
	{
		{X = 516.500000, Y = 896.500000},
	},
	{
		{X = 512.500000, Y = 128.500000},
	},
	{
		{X = 860.500000, Y = 709.500000},
	},
	{
		{X = 177.500000, Y = 313.500000},
	},
	{
		{X = 849.500000, Y = 317.500000},
	},
	{
		{X = 176.500000, Y = 706.500000},
	},
}

local EStore_T2_Locations = {
	{
		--{X = 516.500000, Y = 898.500000},
		{X = 516.500000, Y = 896.500000},
	},
	{
		--{X = 512.500000, Y = 126.500000},
		{X = 512.500000, Y = 128.500000},
	},
	{
		{X = 860.500000, Y = 709.500000},
		--{X = 862.500000, Y = 711.500000},
	},
	{
		{X = 177.500000, Y = 313.500000},
		--{X = 175.500000, Y = 311.500000},
	},
	{
		{X = 849.500000, Y = 317.500000},
		--{X = 851.500000, Y = 315.500000},
	},
	{
		{X = 176.500000, Y = 706.500000},
		--{X = 174.500000, Y = 708.500000},
	},
}

local PGen_T2_Locations = {
	{
		--{X = 520.500000, Y = 898.500000},
		{X = 512.500000, Y = 898.500000},
	},
	{
		--{X = 508.500000, Y = 126.500000},
		{X = 516.500000, Y = 126.500000},
	},
	{
		{X = 858.500000, Y = 713.500000},
		--{X = 864.500000, Y = 707.500000},
	},
	{
		--{X = 173.500000, Y = 315.500000},
		{X = 179.500000, Y = 309.500000},
	},
	{
		{X = 853.500000, Y = 319.500000},
		--{X = 847.500000, Y = 313.500000},
	},
	{
		{X = 172.500000, Y = 704.500000},
		--{X = 178.500000, Y = 710.500000},
	}
}

local Main_Island_Areas = {
	{X1=400,Y1=825,X2=600,Y2=950},
	{X1=400,Y1=75,X2=600,Y2=200},
	{X1=775,Y1=625,X2=925,Y2=775},
	{X1=100,Y1=250,X2=250,Y2=400},
	{X1=775,Y1=250,X2=925,Y2=400},
	{X1=100,Y1=625,X2=250,Y2=775},
}

function OnPopulate()
	ScenarioUtils.InitializeArmies()
	--ScenarioFramework.SetPlayableArea(ScenarioUtils.AreaToRect('AREA_1'))
	for i=1,6 do
		local armyName = 'ARMY_'..i
		local brain = GetBrain(armyName)
		if brain then
			if ArmyIsCivilian(brain:GetArmyIndex()) == false and ArmyIsOutOfGame(brain:GetArmyIndex()) == false then
				FixOrientation(brain)
				if ScenarioInfo.Options.opt_prebuilt_base_brv10 == 1 then		
					SpawnBetterPreBuiltT1Bases(i, brain)
					ForkThread(RemoveStartMassT1)
				elseif ScenarioInfo.Options.opt_prebuilt_base_brv10 == 2 then		
					SpawnBetterPreBuiltT2Bases(i, brain)
					ForkThread(RemoveStartMass)
				end
			end
		end
	end
	
	if ScenarioInfo.Options.opt_prebuilt_base_brv10 == 1 then		
		ReducePhantomAssignmentTime(-225)
		RemoveAllRockReclaim()
	elseif ScenarioInfo.Options.opt_prebuilt_base_brv10 == 2 then		
		ReducePhantomAssignmentTime(-345)
        RemoveMainIslandReclaim()
	end
end

function OnStart(self)

end

function RemoveStartMassT1()
	WaitSeconds(.6)
	for i=1,6 do
		local armyName = 'ARMY_'..i
		local brain = GetBrain(armyName)
		if brain then
			if ArmyIsCivilian(brain:GetArmyIndex()) == false and ArmyIsOutOfGame(brain:GetArmyIndex()) == false then
				--Take all stored resources from players
				brain:TakeResource( 'ENERGY', 2000)
				brain:TakeResource( 'MASS', 250)
			end
		end
	end
end

function RemoveStartMass()
	WaitSeconds(.6)
	for i=1,6 do
		local armyName = 'ARMY_'..i
		local brain = GetBrain(armyName)
		if brain then
			if ArmyIsCivilian(brain:GetArmyIndex()) == false and ArmyIsOutOfGame(brain:GetArmyIndex()) == false then
				--Take all stored resources from players
				brain:TakeResource( 'ENERGY', 4000)
				brain:TakeResource( 'MASS', 650)
			end
		end
	end
end

function GetBrain(name)
	local rt = false
	for k,v in ArmyBrains do
		if(v.Name == name) then
			rt = v
		end			
	end
	return rt
end

function RemoveMainIslandReclaim()
	for k, v in Main_Island_Areas do
		local ents = GetReclaimablesInRect( Rect(v.X1,v.Y1, v.X2, v.Y2 ))
		--Split up tree groups
		for k, ent in ents do
			local bp = ent:GetBlueprint()
			if table.getn(bp.Categories) == 1 and bp.Categories[1] == 'RECLAIMABLE' and bp.ScriptClass == 'TreeGroup' then
				ent:Breakup()
			end
		end
		
		--Remove all Reclaimable props
		 ents = GetReclaimablesInRect( Rect(v.X1,v.Y1, v.X2, v.Y2 ))
		for k, ent in ents do
			local bp = ent:GetBlueprint()
			if table.getn(bp.Categories) == 1 and bp.Categories[1] == 'RECLAIMABLE' then
				ent:SinkAway(-.1)
				ent:Destroy()
			end
		end
	end
end

function RemoveAllRockReclaim()
	local sX, sZ = GetMapSize()
    local ents = GetReclaimablesInRect( Rect(0,0, sX, sZ ))
	
	--Remove all Reclaimable props
    ents = GetReclaimablesInRect( Rect(0,0, sX, sZ ))
    for k, ent in ents do
        local bp = ent:GetBlueprint()
        if table.getn(bp.Categories) == 1 and bp.Categories[1] == 'RECLAIMABLE' and bp.ScriptClass != 'TreeGroup' and bp.ScriptClass != 'Tree' then
            ent:SinkAway(-.1)
            ent:Destroy()
        end
    end
end
function RemoveAllReclaim()
    local sX, sZ = GetMapSize()
    local ents = GetReclaimablesInRect( Rect(0,0, sX, sZ ))
    
    --Split up tree groups
    for k, ent in ents do
        local bp = ent:GetBlueprint()
        if table.getn(bp.Categories) == 1 and bp.Categories[1] == 'RECLAIMABLE' and bp.ScriptClass == 'TreeGroup' then
            ent:Breakup()
        end
    end
    
    --Remove all Reclaimable props
    ents = GetReclaimablesInRect( Rect(0,0, sX, sZ ))
    for k, ent in ents do
        local bp = ent:GetBlueprint()
        if table.getn(bp.Categories) == 1 and bp.Categories[1] == 'RECLAIMABLE' then
            --ent:Kill()
            ent:SinkAway(-.1)
            ent:Destroy()
            --Damage(nil, ent:GetPosition(), ent, 10000, 'Normal')
        end
    end
end

function FixOrientation(armyBrain)
	local commander = armyBrain:GetListOfUnits(categories.COMMAND, false)[1]
	if commander then
		if armyBrain.Name == 'ARMY_1' then
			commander:SetOrientation(OrientFromDir(Vector(0,0,-1)),true)
		elseif armyBrain.Name == 'ARMY_2' then
			commander:SetOrientation(OrientFromDir(Vector(0,0,1)),true)
		elseif armyBrain.Name == 'ARMY_3' then
			commander:SetOrientation(OrientFromDir(Vector(-1,0,-1)),true)
		elseif armyBrain.Name == 'ARMY_4' then
			commander:SetOrientation(OrientFromDir(Vector(1,0,1)),true)
		elseif armyBrain.Name == 'ARMY_5' then
			commander:SetOrientation(OrientFromDir(Vector(-1,0,1)),true)
		elseif armyBrain.Name == 'ARMY_6' then
			commander:SetOrientation(OrientFromDir(Vector(1,0,-1)),true)
		end
	end	
end

function ReducePhantomAssignmentTime(sec)
	if ScenarioInfo.Options.PhantomRevealTime1 == nil then
		LOG('MAP Script: Determined that Phantom-X is not running')
	else
		LOG('MAP Script: Determined that Phantom-X is running')
		LOG('MAP Script: Changing assignment time...')
		
		import('/lua/PhantomSim.lua').ChangeAssignmentTime(sec)
	end
end

function SpawnBetterPreBuiltT1Bases(idx, brain)
	local factionIndex = brain:GetFactionIndex()
	local resourceStructures = nil
	local pgen = nil
	local fac = nil
	local posX, posY = brain:GetArmyStartPos()
	local t1engy = nil

	if factionIndex == 1 then
		resourceStructures = { 'UEB1202', 'UEB1103', 'UEB1103', 'UEB1103', 'UEB1103', 'UEB1103', 'UEB1103', 'UEB1103', 'UEB1103', 'UEB1103' }
		pgen = 'UEB1101'
		fac = 'UEB0101'
		estore = 'UEB1105'
		mstore = 'UEB1106'
		t1engy = 'UEL0105'
	elseif factionIndex == 2 then
		resourceStructures = { 'UAB1202', 'UAB1103', 'UAB1103', 'UAB1103', 'UAB1103', 'UAB1103', 'UAB1103', 'UAB1103', 'UAB1103', 'UAB1103' }
		pgen = 'UAB1101'
		fac = 'UAB0101'
		estore = 'UAB1105'
		mstore = 'UAB1106'
		t1engy = 'UAL0105'
	elseif factionIndex == 3 then
		resourceStructures = { 'URB1202', 'URB1103', 'URB1103', 'URB1103', 'URB1103', 'URB1103', 'URB1103', 'URB1103', 'URB1103', 'URB1103' }
		pgen = 'URB1101'
		fac = 'URB0101'
		estore = 'URB1105'
		mstore = 'URB1106'
		t1engy = 'URL0105'
	elseif factionIndex == 4 then
		resourceStructures = { 'XSB1202', 'XSB1103', 'XSB1103', 'XSB1103', 'XSB1103', 'XSB1103', 'XSB1103', 'XSB1103', 'XSB1103', 'XSB1103' }
		pgen = 'XSB1101'
		fac = 'XSB0101'
		estore = 'XSB1105'
		mstore = 'XSB1106'
		t1engy = 'XSL0105'
	end
	
	local midPosition = GetMyMiddleIsland(brain.Name)
	local unit = brain:CreateResourceBuildingNearest(resourceStructures[9], midPosition[1], midPosition[3])
	if unit != nil and unit:GetBlueprint().Physics.FlattenSkirt then
		unit:CreateTarmac(true, true, true, false, false)
		local resLoc = unit:GetPosition()
		CreateUnitHPR( t1engy, brain.Name, resLoc[1] - 4, GetTerrainHeight( resLoc[1] - 4, resLoc[3]) , resLoc[3], 0,0,0)
	end
	
	if resourceStructures then
		-- place resource structures down
		for k, v in resourceStructures do
			local unit = nil
			if idx == 1 and k == 1 then --Patch for mex position in ARMY_1's base
				unit = brain:CreateResourceBuildingNearest(v, posX-5, posY+5)
			elseif idx == 2 and k == 1 then --Patch for mex position in ARMY_2's base
				unit = brain:CreateResourceBuildingNearest(v, posX+5, posY-5)
			elseif idx == 5 and k == 1 then --Patch for mex position in ARMY_5's base
				unit = brain:CreateResourceBuildingNearest(v, posX+5, posY)
			else
				unit = brain:CreateResourceBuildingNearest(v, posX, posY)
			end
			if unit != nil and unit:GetBlueprint().Physics.FlattenSkirt then
				unit:CreateTarmac(true, true, true, false, false)
				local resLoc = unit:GetPosition()
				--if k == 1 then
					--Create Mass Storage
					--CreateUnitHPR( mstore, brain.Name, resLoc[1] + 2, GetTerrainHeight( resLoc[1] + 2, resLoc[3]) , resLoc[3], 0,0,0)
					--CreateUnitHPR( mstore, brain.Name, resLoc[1] - 2, GetTerrainHeight( resLoc[1] - 2, resLoc[3]) , resLoc[3], 0,0,0)
					--CreateUnitHPR( mstore, brain.Name, resLoc[1], GetTerrainHeight( resLoc[1], resLoc[3] + 2) , resLoc[3] + 2, 0,0,0)
					--CreateUnitHPR( mstore, brain.Name, resLoc[1], GetTerrainHeight( resLoc[1], resLoc[3] - 2) , resLoc[3] - 2, 0,0,0)
				--end
				CreateUnitHPR( t1engy, brain.Name, resLoc[1] - 4, GetTerrainHeight( resLoc[1] - 4, resLoc[3]) , resLoc[3], 0,0,0)
			end
		end
	end
	
	local f_loc = Factory_Locations[idx]
	CreateUnitHPR( fac, brain.Name, f_loc.X, GetTerrainHeight( f_loc.X, f_loc.Y ) , f_loc.Y, 0,0,0)
	
	for k, v in PGen_Locations[idx] do
		CreateUnitHPR( pgen, brain.Name, v.X, GetTerrainHeight( v.X, v.Y ) , v.Y, 0,0,0)
	end
	
	--for k, v in EStore_T1_Locations[idx] do
	--	CreateUnitHPR( estore, brain.Name, v.X, GetTerrainHeight( v.X, v.Y ) , v.Y, 0,0,0)
	--end
end

function SpawnBetterPreBuiltT2Bases(idx, brain)
	local factionIndex = brain:GetFactionIndex()
	local resourceStructures = nil
	local pgen = nil
	local fac = nil
	local estore = nil
	local mstore = nil
	local t1engy = nil
	local posX, posY = brain:GetArmyStartPos()

	if factionIndex == 1 then
		resourceStructures = { 'UEB1202', 'UEB1202', 'UEB1202', 'UEB1202', 'UEB1202', 'UEB1103', 'UEB1103', 'UEB1103', 'UEB1103', 'UEB1103' }
		pgen = 'UEB1201'
		fac = 'UEB0101'
		estore = 'UEB1105'
		mstore = 'UEB1106'
		t1engy = 'UEL0105'
	elseif factionIndex == 2 then
		resourceStructures = { 'UAB1202', 'UAB1202', 'UAB1202', 'UAB1202', 'UAB1202', 'UAB1103', 'UAB1103', 'UAB1103', 'UAB1103', 'UAB1103' }
		pgen = 'UAB1201'
		fac = 'UAB0101'
		estore = 'UAB1105'
		mstore = 'UAB1106'
		t1engy = 'UAL0105'
	elseif factionIndex == 3 then
		resourceStructures = { 'URB1202', 'URB1202', 'URB1202', 'URB1202', 'URB1202', 'URB1103', 'URB1103', 'URB1103', 'URB1103', 'URB1103' }
		pgen = 'URB1201'
		fac = 'URB0101'
		estore = 'URB1105'
		mstore = 'URB1106'
		t1engy = 'URL0105'
	elseif factionIndex == 4 then
		resourceStructures = { 'XSB1202', 'XSB1202', 'XSB1202', 'XSB1202', 'XSB1202', 'XSB1103', 'XSB1103', 'XSB1103', 'XSB1103', 'XSB1103' }
		pgen = 'XSB1201'
		fac = 'XSB0101'
		estore = 'XSB1105'
		mstore = 'XSB1106'
		t1engy = 'XSL0105'
	end
	
	local midPosition = GetMyMiddleIsland(brain.Name)
	local unit = brain:CreateResourceBuildingNearest(resourceStructures[9], midPosition[1], midPosition[3])
	
	if unit != nil and unit:GetBlueprint().Physics.FlattenSkirt then
		unit:CreateTarmac(true, true, true, false, false)
		--Create Mass Storage
		local resLoc = unit:GetPosition()
		--CreateUnitHPR( mstore, brain.Name, resLoc[1] + 2, GetTerrainHeight( resLoc[1] + 2, resLoc[3]) , resLoc[3], 0,0,0)
		--CreateUnitHPR( mstore, brain.Name, resLoc[1] - 2, GetTerrainHeight( resLoc[1] - 2, resLoc[3]) , resLoc[3], 0,0,0)
		--CreateUnitHPR( mstore, brain.Name, resLoc[1], GetTerrainHeight( resLoc[1], resLoc[3] + 2) , resLoc[3] + 2, 0,0,0)
		--CreateUnitHPR( mstore, brain.Name, resLoc[1], GetTerrainHeight( resLoc[1], resLoc[3] - 2) , resLoc[3] - 2, 0,0,0)
		
		--Create T1 engies around mass spots
		CreateUnitHPR( t1engy, brain.Name, resLoc[1] - 4, GetTerrainHeight( resLoc[1] - 4, resLoc[3]) , resLoc[3], 0,0,0)
		CreateUnitHPR( t1engy, brain.Name, resLoc[1] + 4, GetTerrainHeight( resLoc[1] + 4, resLoc[3]) , resLoc[3], 0,0,0)
	end
	
	local isFirstMex = true
	if resourceStructures then
		-- place resource structures down
		for k, v in resourceStructures do
			local unit
			if idx == 1 and k == 5 then --Patch for stupid mex position in ARMY_1's base
				unit = brain:CreateResourceBuildingNearest(v, posX-15, posY-15)
			elseif idx == 6 and k == 5 then --Patch for stupid mex position in ARMY_6's base
				unit = brain:CreateResourceBuildingNearest(v, posX, posY-15)
			else
				unit = brain:CreateResourceBuildingNearest(v, posX, posY)
			end
			
			if unit != nil and unit:GetBlueprint().Physics.FlattenSkirt then
				unit:CreateTarmac(true, true, true, false, false)
				local resLoc = unit:GetPosition()
				if isFirstMex then
					--Create Mass Storage
					CreateUnitHPR( mstore, brain.Name, resLoc[1] + 2, GetTerrainHeight( resLoc[1] + 2, resLoc[3]) , resLoc[3], 0,0,0)
					--CreateUnitHPR( mstore, brain.Name, resLoc[1] - 2, GetTerrainHeight( resLoc[1] - 2, resLoc[3]) , resLoc[3], 0,0,0)
					--CreateUnitHPR( mstore, brain.Name, resLoc[1], GetTerrainHeight( resLoc[1], resLoc[3] + 2) , resLoc[3] + 2, 0,0,0)
					--CreateUnitHPR( mstore, brain.Name, resLoc[1], GetTerrainHeight( resLoc[1], resLoc[3] - 2) , resLoc[3] - 2, 0,0,0)
					isFirstMex = false
				end
				
				--Create T1 engies around mass spots
				CreateUnitHPR( t1engy, brain.Name, resLoc[1] - 4, GetTerrainHeight( resLoc[1] - 4, resLoc[3]) , resLoc[3], 0,0,0)
				CreateUnitHPR( t1engy, brain.Name, resLoc[1] + 4, GetTerrainHeight( resLoc[1] + 4, resLoc[3]) , resLoc[3], 0,0,0)
			end
		end
	end
	
	local f_loc = Factory_Locations[idx]
	CreateUnitHPR( fac, brain.Name, f_loc.X, GetTerrainHeight( f_loc.X, f_loc.Y ) , f_loc.Y, 0,0,0)
	
	for k, v in PGen_T2_Locations[idx] do
		CreateUnitHPR( pgen, brain.Name, v.X, GetTerrainHeight( v.X, v.Y ) , v.Y, 0,0,0)
	end
	
	for k, v in EStore_T2_Locations[idx] do
		CreateUnitHPR( estore, brain.Name, v.X, GetTerrainHeight( v.X, v.Y ) , v.Y, 0,0,0)
	end
	
	--Give Comm T2 Engineering Upgrade
	local commander = brain:GetListOfUnits(categories.COMMAND, false)[1]
	if commander then
		commander:CreateEnhancement('AdvancedEngineering')
		
		--Create T1 engies around Comm
		--local commLoc = commander:GetPosition()
		--CreateUnitHPR( t1engy, brain.Name, commLoc[1] - 2, GetTerrainHeight( commLoc[1] - 2, commLoc[3]) , commLoc[3], 0,0,0)
		--CreateUnitHPR( t1engy, brain.Name, commLoc[1] + 2, GetTerrainHeight( commLoc[1] + 2, commLoc[3]) , commLoc[3], 0,0,0)
		--CreateUnitHPR( t1engy, brain.Name, commLoc[1], GetTerrainHeight( commLoc[1], commLoc[3] + 2) , commLoc[3] + 2, 0,0,0)
		--CreateUnitHPR( t1engy, brain.Name, commLoc[1], GetTerrainHeight( commLoc[1], commLoc[3] - 2) , commLoc[3] - 2, 0,0,0)
		--CreateUnitHPR( t1engy, brain.Name, commLoc[1] - 2, GetTerrainHeight( commLoc[1] - 2, commLoc[3] + 2) , commLoc[3] + 2, 0,0,0)
		--CreateUnitHPR( t1engy, brain.Name, commLoc[1] - 2, GetTerrainHeight( commLoc[1] - 2, commLoc[3] - 2) , commLoc[3] - 2, 0,0,0)
		--CreateUnitHPR( t1engy, brain.Name, commLoc[1] + 2, GetTerrainHeight( commLoc[1] + 2, commLoc[3] + 2) , commLoc[3] + 2, 0,0,0)
		--CreateUnitHPR( t1engy, brain.Name, commLoc[1] + 2, GetTerrainHeight( commLoc[1] + 2, commLoc[3] - 2) , commLoc[3] - 2, 0,0,0)
	end
	
	
end

function GetMyMiddleIsland(name)
	local rt
	if name == 'ARMY_1' then
		rt = ScenarioUtils.GetMarker('Island 23').position
	elseif name == 'ARMY_2' then
		rt = ScenarioUtils.GetMarker('Island 20').position
	elseif name == 'ARMY_3' then
		rt = ScenarioUtils.GetMarker('Island 18').position
	elseif name == 'ARMY_4' then
		rt = ScenarioUtils.GetMarker('Island 21').position
	elseif name == 'ARMY_5' then
		rt = ScenarioUtils.GetMarker('Island 19').position
	elseif name == 'ARMY_6' then
		rt = ScenarioUtils.GetMarker('Island 22').position
	end
	return rt
end

function SpawnTents()
	local tent = "xrb0304"
	local tentnum = ScenarioInfo.Options.opt_tents

	if(StartingPlayersExistance.ARMY_1 == true) then
		local x = TentsLocation.Player1.TentX
		local y = TentsLocation.Player1.TentY
		if tentnum > 0 then
			CreateUnitHPR( tent, "ARMY_1", x, GetTerrainHeight( x, y ) , y, 0,0,0)
		end
		if tentnum > 1 then
			CreateUnitHPR( tent, "ARMY_1", x, GetTerrainHeight( x, y+2 ), y+2, 0,0,0)
		end
		if tentnum > 2 then
			CreateUnitHPR( tent, "ARMY_1", x + 2, GetTerrainHeight( x+2, y), y, 0,0,0)
		end
		if tentnum > 3 then
			CreateUnitHPR( tent, "ARMY_1", x + 2, GetTerrainHeight( x+2, y+2 ), y + 2, 0,0,0)
		end
		if tentnum > 4 then
			CreateUnitHPR( tent, "ARMY_1", x - 2, GetTerrainHeight( x-2, y ), y, 0,0,0)
		end
		if tentnum > 5 then
			CreateUnitHPR( tent, "ARMY_1", x - 2, GetTerrainHeight( x-2, y+2 ), y + 2, 0,0,0)
		end
	end
	if(StartingPlayersExistance.ARMY_2 == true) then
		local x = TentsLocation.Player2.TentX
		local y = TentsLocation.Player2.TentY
		if tentnum > 0 then
			CreateUnitHPR( tent, "ARMY_2", x, GetTerrainHeight( x, y ) , y, 0,0,0)
		end
		if tentnum > 1 then
			CreateUnitHPR( tent, "ARMY_2", x, GetTerrainHeight( x, y-2 ), y-2, 0,0,0)
		end
		if tentnum > 2 then
			CreateUnitHPR( tent, "ARMY_2", x - 2, GetTerrainHeight( x-2, y), y, 0,0,0)
		end
		if tentnum > 3 then
			CreateUnitHPR( tent, "ARMY_2", x - 2, GetTerrainHeight( x-2, y-2 ), y - 2, 0,0,0)
		end
		if tentnum > 4 then
			CreateUnitHPR( tent, "ARMY_2", x + 2, GetTerrainHeight( x+2, y ), y, 0,0,0)
		end
		if tentnum > 5 then
			CreateUnitHPR( tent, "ARMY_2", x + 2, GetTerrainHeight( x+2, y-2 ), y - 2, 0,0,0)
		end
	end
	if(StartingPlayersExistance.ARMY_3 == true) then
		local x = TentsLocation.Player3.TentX
		local y = TentsLocation.Player3.TentY
		if tentnum > 0 then
			CreateUnitHPR( tent, "ARMY_3", x, GetTerrainHeight( x, y) , y, 0,0,0)
		end
		if tentnum > 1 then
			CreateUnitHPR( tent, "ARMY_3", x+2, GetTerrainHeight( x+2, y+2 ), y+2, 0,0,0)
		end
		if tentnum > 2 then
			CreateUnitHPR( tent, "ARMY_3", x + 2, GetTerrainHeight( x+2, y), y, 0,0,0)
		end
		if tentnum > 3 then
			CreateUnitHPR( tent, "ARMY_3", x + 4, GetTerrainHeight( x+4, y+2 ), y + 2, 0,0,0)
		end
		if tentnum > 4 then
			CreateUnitHPR( tent, "ARMY_3", x, GetTerrainHeight( x, y+2 ), y+2, 0,0,0)
		end
		if tentnum > 5 then
			CreateUnitHPR( tent, "ARMY_3", x+2, GetTerrainHeight( x+2, y+4 ), y+4, 0,0,0)
		end
	end
	if(StartingPlayersExistance.ARMY_4 == true) then
		local x = TentsLocation.Player4.TentX
		local y = TentsLocation.Player4.TentY
		if tentnum > 0 then
			CreateUnitHPR( tent, "ARMY_4", x, GetTerrainHeight( x, y) , y, 0,0,0)
		end
		if tentnum > 1 then
			CreateUnitHPR( tent, "ARMY_4", x-2, GetTerrainHeight( x-2, y-2 ), y-2, 0,0,0)
		end
		if tentnum > 2 then
			CreateUnitHPR( tent, "ARMY_4", x - 2, GetTerrainHeight( x-2, y), y, 0,0,0)
		end
		if tentnum > 3 then
			CreateUnitHPR( tent, "ARMY_4", x - 4, GetTerrainHeight( x-4, y-2 ), y - 2, 0,0,0)
		end
		if tentnum > 4 then
			CreateUnitHPR( tent, "ARMY_4", x, GetTerrainHeight( x, y-2 ), y-2, 0,0,0)
		end
		if tentnum > 5 then
			CreateUnitHPR( tent, "ARMY_4", x-2, GetTerrainHeight( x-2, y-4 ), y-4, 0,0,0)
		end
	end
	if(StartingPlayersExistance.ARMY_5 == true) then
		local x = TentsLocation.Player5.TentX
		local y = TentsLocation.Player5.TentY
		if tentnum > 0 then
			CreateUnitHPR( tent, "ARMY_5", x, GetTerrainHeight( x, y) , y, 0,0,0)
		end
		if tentnum > 1 then
			CreateUnitHPR( tent, "ARMY_5", x+2, GetTerrainHeight( x+2, y-2 ), y-2, 0,0,0)
		end
		if tentnum > 2 then
			CreateUnitHPR( tent, "ARMY_5", x, GetTerrainHeight( x, y-2), y-2, 0,0,0)
		end
		if tentnum > 3 then
			CreateUnitHPR( tent, "ARMY_5", x + 2, GetTerrainHeight( x+2, y-4 ), y - 4, 0,0,0)
		end
		if tentnum > 4 then
			CreateUnitHPR( tent, "ARMY_5", x+2, GetTerrainHeight( x+2, y), y, 0,0,0)
		end
		if tentnum > 5 then
			CreateUnitHPR( tent, "ARMY_5", x+4, GetTerrainHeight( x+4, y-2 ), y-2, 0,0,0)
		end
	end
	if(StartingPlayersExistance.ARMY_6 == true) then
		local x = TentsLocation.Player6.TentX
		local y = TentsLocation.Player6.TentY
		if tentnum > 0 then
			CreateUnitHPR( tent, "ARMY_6", x, GetTerrainHeight( x, y) , y, 0,0,0)
		end
		if tentnum > 1 then
			CreateUnitHPR( tent, "ARMY_6", x-2, GetTerrainHeight( x-2, y+2 ), y+2, 0,0,0)
		end
		if tentnum > 2 then
			CreateUnitHPR( tent, "ARMY_6", x, GetTerrainHeight( x, y+2), y+2, 0,0,0)
		end
		if tentnum > 3 then
			CreateUnitHPR( tent, "ARMY_6", x-2, GetTerrainHeight( x-2, y+4 ), y+4, 0,0,0)
		end
		if tentnum > 4 then
			CreateUnitHPR( tent, "ARMY_6", x-2, GetTerrainHeight( x-2, y), y, 0,0,0)
		end
		if tentnum > 5 then
			CreateUnitHPR( tent, "ARMY_6", x-4, GetTerrainHeight( x-4, y+2 ), y+2, 0,0,0)
		end
	end
end
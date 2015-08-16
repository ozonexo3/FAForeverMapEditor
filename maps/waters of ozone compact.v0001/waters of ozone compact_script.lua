local ScenarioUtils = import('/lua/sim/ScenarioUtilities.lua')
local ScenarioFramework = import('/lua/ScenarioFramework.lua')

   function ScenarioUtils.CreateResources()
   	-- fetch markers and iterate them
   	local markers = ScenarioUtils.GetMarkers();
   	for i, tblData in pairs(markers) do
   		-- spawn resources?
		local numberOfSpots = 0;
   		local doit = false;
		local Armies = ListArmies();
   		if (tblData.resource and not tblData.SpawnWithArmy) then
   			-- standard resources, spawn it
   			doit = true;
   		elseif (tblData.resource and tblData.SpawnWithArmy) then
   			-- resources bound to player, check if army is presend
   			for j, army in ListArmies() do
				if(tblData.SpawnWithArmy == "ARMY_12") then
					if(army == "ARMY_1") then
						if(numberOfSpots > 0) then
							doit = true;
							break;
						else
							numberOfSpots = 1;
							doit = false;
						end
					elseif(army == "ARMY_2") then
						if(numberOfSpots > 0) then
							doit = true;
							break;
						else
							numberOfSpots = 1;
							doit = false;
						end
					else
						doit = false;
					end
				elseif(tblData.SpawnWithArmy == "ARMY_34") then
					if(army == "ARMY_3") then
						if(numberOfSpots > 0) then
							doit = true;
							break;
						else
							numberOfSpots = 1;
							doit = false;
						end
					elseif(army == "ARMY_4") then
						if(numberOfSpots > 0) then
							doit = true;
							break;
						else
							numberOfSpots = 1;
							doit = false;
						end
					else
						doit = false;
					end
				elseif(tblData.SpawnWithArmy == "ARMY_56") then
					if(army == "ARMY_5") then
						if(numberOfSpots > 0) then
							doit = true;
							break;
						else
							numberOfSpots = 1;
							doit = false;
						end
					elseif(army == "ARMY_6") then
						if(numberOfSpots > 0) then
							doit = true;
							break;
						else
							numberOfSpots = 1;
							doit = false;
						end
					else
						doit = false;
					end
				elseif(tblData.SpawnWithArmy == "ARMY_78") then
					if(army == "ARMY_7") then
						if(numberOfSpots > 0) then
							doit = true;
							break;
						else
							numberOfSpots = 1;
							doit = false;
						end
					elseif(army == "ARMY_8") then
						if(numberOfSpots > 0) then
							doit = true;
							break;
						else
							numberOfSpots = 1;
							doit = false;
						end
					else
						doit = false;
					end					
   				elseif (tblData.SpawnWithArmy == army) then
   					doit = true;  -- we made sure the army is present, allow spawn
   					break;
   				end
   			end
   		end
		local Armies = ListArmies();
		if(Armies < 4) then
			doit = false;
		end
   		
   		if (doit) then
   			-- check type of resource and set parameters
   			local bp, albedo, sx, sz, lod;
   			if tblData.type == "Mass" then
   				albedo = "/env/common/splats/mass_marker.dds";
   				bp = "/env/common/props/massDeposit01_prop.bp";
   				sx = 2;
   				sz = 2;
   				lod = 100;
   			else
   				albedo = "/env/common/splats/hydrocarbon_marker.dds";
   				bp = "/env/common/props/hydrocarbonDeposit01_prop.bp";
   				sx = 6;
   				sz = 6;
   				lod = 200;
   			end
   			-- create the resource
   			CreateResourceDeposit(tblData.type,	tblData.position[1], tblData.position[2], tblData.position[3], tblData.size);
   			-- create the resource graphic on the map
   			CreatePropHPR(bp, tblData.position[1], tblData.position[2], tblData.position[3], Random(0,360), 0, 0);
   			-- create the resource icon on the map
   			CreateSplat(
   				tblData.position,           # Position
   				0,                          # Heading (rotation)
   				albedo,                     # Texture name for albedo
   				sx, sz,                     # SizeX/Z
   				lod,                        # LOD
   				0,                          # Duration (0 == does not expire)
   				-1,                         # army (-1 == not owned by any single army)
   				0							# ???
   			);
   		end
   	end
   end

function OnPopulate()
    ScenarioUtils.InitializeArmies()
    ScenarioFramework.SetPlayableArea('AREA_1' , false)
end
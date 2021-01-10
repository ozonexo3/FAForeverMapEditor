---
---
---
patterns = {

    ---
    --- short waves
    ---
    {
        name = 'short waves',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            --- 
            ---
            {
                texture = "/env/common/decals/shoreline/wavetest.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0.0, 0.0, 0.0, },
                period = 3.3,
                periodVariance = 0.1,
                speed = 0.015,
                speedVariance = 0.005,
                lifetime = 33.0,
                lifetimeVariance = 1.0,
                scale = { 0.5, 1.2 },
                scaleVariance = 0.0,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1, 
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },

    ---
    --- short waves
    ---
    {
        name = 'Alt shore',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            --- 
            ---
            {
                texture = "/env/common/decals/AltaShore001_normals.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0.0, 20.0, 0.0, },
                period = 6.3,
                periodVariance = 0.1,
                speed = 0.15,
                speedVariance = 0.005,
                lifetime = 33.0,
                lifetimeVariance = 1.0,
                scale = { 0.5, 1.2 },
                scaleVariance = 0.0,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1, 
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    
    ---
    --- medium waves
    ---
    {
        name = 'medium waves',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/wavetest.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0.0, 0.0, 0.0, },
                period = 3.3,
                periodVariance = 0.1,
                speed = 0.02,
                speedVariance = 0.005,
                lifetime = 66.0,
                lifetimeVariance = 1.0,
                scale = { 0.3, 0.6 },
                scaleVariance = 0.001,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,   
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    
    ---
    --- long waves
    ---
    {
        name = 'long waves',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/wavetest.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0.0, 0.0, 0.0, },
                period = 6.6,
                periodVariance = 0.0,
                speed = 0.02,
                speedVariance = 0.005,
                lifetime = 132.0,
                lifetimeVariance = 3.0,
                scale = { 0.8, 1.7 },
                scaleVariance = 0.001,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    
    ---
    --- receding waves
    ---
    {
        name = 'receding waves',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/waverecedetest.dds",
                ramp = "/env/common/decals/shoreline/waveramprecedetest.dds",
                position = { 0.0, 0.0, 0.0, },
                period = 1,
                periodVariance = 0.0,
                speed = -0.008,
                speedVariance = 0.005,
                lifetime = 17.0,
                lifetimeVariance = 1.0,
                scale = { 0.5, 0.8 },
                scaleVariance = 0.001,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    ---
    --- Turbulance
    ---
    {
        name = 'Turbulance',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/turbulance02_albedo.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0, 0, 0, },
                period = 5,
                periodVariance = .5,
                speed = -.007,
                speedVariance = 0.005,
                lifetime = 50.0,
                lifetimeVariance = 20,
                scale = { .8, 1.5 },
                scaleVariance = 0.5,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    ---
    --- Incomming2
    ---
    {
        name = 'Incomming2',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/turbulance03_albedo.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0, 0, 0, },
                period = 12,
                periodVariance = 2,
                speed = .02,
                speedVariance = 0.005,
                lifetime = 130.0,
                lifetimeVariance = 20,
                scale = { .5, 5 },
                scaleVariance = 0.5,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    ---
    --- Incomming
    ---
    {
        name = 'Incomming',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/turbulance03_albedo.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0, 0, 0, },
                period = 10,
                periodVariance = 2,
                speed = .02,
                speedVariance = 0.005,
                lifetime = 130.0,
                lifetimeVariance = 20,
                scale = { .5, 5 },
                scaleVariance = 0.5,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    ---
    --- Foam
    ---
    {
        name = 'Foam',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/turbulance04_albedo.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0, 0, 0, },
                period = 6,
                periodVariance = 1,
                speed = .00,
                speedVariance = 0.000,
                lifetime = 20.0,
                lifetimeVariance = 3,
                scale = { .5, 1 },
                scaleVariance = 0.1,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    ---
    --- Ian Test waves
    ---
    {
        name = 'Ian Test Waves',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/shoreBreakTest.dds",
                ramp = "/env/common/decals/shoreline/waveramptest.dds",
                position = { 0.0, 0.0, 0.0, },
                period = 10,
                periodVariance = 1,
                speed = 0.02,
                speedVariance = 0.005,
                lifetime = 150.0,
                lifetimeVariance = 20,
                scale = { .8, 2 },
                scaleVariance = 0.5,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1,
            },
        },
    },
    {
        name = 'Animated shore break',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/wave05_albedo.dds",
                ramp = "/env/common/decals/shoreline/ramp_waves_01.dds",
                position = { 0.0, 0.0, 0.0, },
                period = 10,
                periodVariance = 4,
                speed = 0.04,
                speedVariance = 0.02,
                lifetime = 65.0,
                lifetimeVariance = 50.0,
                scale = { .8, 2.4 },
                scaleVariance = 0.5,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 16,
                frameRate = 0.47,
                frameRateVariance = 0.047,
                stripCount = 2, 
            },
        },
    }, 

    {
        name = 'Animated lava - large',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/maps/NMCA_005/env/decals/animated-lava-1.dds",
                ramp = "/env/common/decals/shoreline/ramp_waves_01.dds",
                position = { 0.0, -2.0, 0.0, },
                period = 5,
                periodVariance = 2,
                speed = 0.01,
                speedVariance = 0.01,
                lifetime = 24.0,
                lifetimeVariance = 6.0,
                scale = { 1.8, 2.4 },
                scaleVariance = 0.5,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 64,
                frameRate = 2,
                frameRateVariance = 0.0,
                stripCount = 1, 
            },
        },
    }, 
    {
        name = 'Animated lava - large2',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/maps/NMCA_005/env/decals/animated-lava-1.dds",
                ramp = "/env/common/decals/shoreline/ramp_waves_01.dds",
                position = { 0.0, -2.0, 0.0, },
                period = 5,
                periodVariance = 2,
                speed = 0.01,
                speedVariance = 0.01,
                lifetime = 65.0,
                lifetimeVariance = 50.0,
                scale = { .8, 2.4 },
                scaleVariance = 0.5,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 8,
                frameRate = 2,
                frameRateVariance = 0.5,
                stripCount = 2, 
            },
        },
    },
    {
        name = 'Long slow break',
        preview = '/editor/tools/water/nopreview.bmp',
        parameters = {
            ---
            ---
            ---
            {
                texture = "/env/common/decals/shoreline/wave06_albedo.dds",
                ramp = "/env/common/decals/shoreline/ramp_waves_01.dds",
                position = { 0.0, 0.0, 0.0, },
                period = 30,
                periodVariance = 20,
                speed = 0.02,
                speedVariance = 0.01,
                lifetime = 165.0,
                lifetimeVariance = 100.0,
                scale = { 4.8, 5.8 },
                scaleVariance = 0.5,
                velocityDelta = { 0.0, 0.0, 0.0, },
                frameCount = 1,
                frameRate = 1,
                frameRateVariance = 0,
                stripCount = 1, 
            },
        },
    },          
}
		
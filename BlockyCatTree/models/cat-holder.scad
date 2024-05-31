$fn=60;
slices = 50;
slice_height = 1;
min_radius = 10;
max_radius = 40;
steepness = 0.4;
transition = 0.6;

function sigmoid(x, steepness = 0.5, transition = 0.5) =
    let (
        increment      = 1.0 - pow( steepness, 0.1),
        starting_point = -transition / increment        
    ) 1 / ( 1 + exp( -( x / increment + starting_point) ) );

s_factors = [ for (i = [0:slices]) 
    1.0 - sigmoid( i / slices, steepness, transition ) 
];

module outer_shape() {
    for (i = [0:slices]) {
        translate([0, 0, i * slice_height]) {
            cylinder(h=slice_height, r=min_radius + (1.0-s_factors[i]) * (max_radius - min_radius));
        }
    }
}

module inner_shape() {
    difference() {
        translate([0,0,slice_height*(slices * 1.2 + 5)]) {
            sphere(r=max_radius);
        }
        cube(slice_height * 75, center=true);
    }
}

difference()
{
    outer_shape();
    inner_shape();
}


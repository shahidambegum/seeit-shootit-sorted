extends Area2D

@export var speed = 600
var direction = Vector2.RIGHT  # Default direction is to the right

func _ready():
	# Make sure the bullet direction is set when the gun is instantiated
	set_direction(direction)

func set_direction(dir: Vector2):
	# Set the direction of the bullet
	direction = dir

func _physics_process(delta):
	# Move the bullet in the direction it's facing
	global_position.x += direction.x * speed * delta

func _on_visible_on_screen_notifier_2d_screen_exited():
	queue_free()

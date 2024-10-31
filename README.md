# Overview
The Cheese Touch is a 2D fighting game based on the Diary of a Wimpy Kid series authored by Jeff Kinney. "The Cheese Touch" is a concept that appears in Diary of a Wimpy Kid: Rodrick Rules, and is kind of their version of "cooties". One day, a piece of cheese fell onto the
basketball court floor, slowly rotting and gathering mold over several days. After the cheese was sufficiently nasty, a kid named Darren decided he was going to touch it. From then on, the cheese touch could be transferred from child to child, with all other students avoiding
whoever has it like the plague.

# Our Adaptation
Instead of the cheese touch being a cooties-like game, we decided to make it a zombie infection. Darren is patient zero to this plague, touching the cheese and subsequently infecting everybody else in the school. The player can choose between the Heffley family or Rowley, each with 
unique and different stats that make your decision crucial to succeed. Zombies will spawn and attempt to surround, swarm, and infect the player. In order to win, you must defeat all of the zombies and keep moving rightward and progressing through the school, eventually fighting 
Darren in an epic boss battle (not implemented yet)

# How to Play
**WASD**: Move up, down, left, right  
**Shift**: Sprint  
**Spacebar**: Jump  
**Left Click**: Attack  


# Design Process: Basic Mechanics
### Character Sprite & Movement Setup
The first thing Kevin and I decided to implement in our game were the core movement mechanics and the sprites for the player characters. This was not much of an issue as it was very similar to the Bomber example we made in class. The hardest 
part here was cutting all of the characters from the internet, filling in their background so they aren't transparent, and removing the blue paper lines from behind the character. One main issue I ran into here was that not
all characters are easy to find on the internet. Often times the photos that I found were only of the top half of their body or they were wearing a weird outfit; something that was different from their default style. 
To fix this slight issue, I had to grab a few side characters from the books, take pictures of them, and crop them myself, as opposed to finding a PNG online. With this naturally came the ability to attack, which is managed through a deactivated-by-default square with a box
collider. When the player clicks, this collider is set to active and we check if the box collides with an enemy.
  
### Character Selector
Now that we had a basic setup for our player spawning, we knew we wanted to implement the ability for the user to choose their character. We made a new scene and added some custom buttons that were made in Photoshop, and gave each character their own unique stats.
The UI management in Unity is super easy so all this took was a script with functions that set our player stats based on the button that we press. This then launches the player into level one with the selected character. After we had this working, we 
wanted some visual feedback on how much health the user has, so we added a health bar. Again, this was very easy with a tutorial on YouTube, so not many problems there. We also decided to add a stamina bar as it was a bit ambiguous to tell when you were sprinting
without the bar for visual feedback.

### Level One Artwork
Once our movement, sprites, and character selecting were implemented, we began work on the map for level one. The art for level one was created in Photoshop; we knew we wanted the first level to be set on the basketball court, so I searched the books for images of the basketball court. 
Every image I could find only showed around 1/3 to 1/2 of the court, so I had to take some creative liberty and draw a lot of the map from scratch. Given more time, these maps would have more objects scattered around 1) for visual aesthetics and 2) we could spawn
weapons that the player can use around the map. After all of this, I clamped the player's movement to within the map, and the setup for level one was completed.

# Design Process: The Zombies
### Spawning
Moving forward, we had basic movement and a basic level to start the foundation of our zombie spawning. We both agreed that in the first level, zombies should spawn from the right side of the map only. I set up a 2D object on the right and calculate its location and bounds
and randomly spawn a zombie sprite somewhere within these bounds. We loop through the amount of zombies to spawn until they are all spawned, and now we had a working zombie spawner.   

### Movement
Next, we had to make the zombies move toward the player, which came with it's own host of problems. To begin, I was just using the zombie's transform to move them, no rigid bodies involved. We didn't want to complicate our system with unneccesary components so we thought
rigid bodies were a bad idea. As we soon found out, the rigid bodies were somewhat essential for our games vision. Currently, with no RB2D, all of the zombies would stack on top of eachother and it was impossible to tell how many zombies you were fighting. To combat this, I 
attached a rigid body to each zombie so that they could collide with each other. I added a new physics material to them with very low bounciness and friction, causing them to slide around each other and "surround" the player, as opposed to clumping up all on one side.   
  
### Attacking
Obviously, a crucial aspect of zombies is their ability to hurt the player. We originally started the enemy attacking script by checking if they collide with the player and lowering the player's health based on that. This method was problematic, however, as it 
was very difficult to win. The zombies would often crowd around the player, and with no indicator/delay in attacking, you would just die nearly instantly. To combat this, I added a few key systems:  
  
**Enemy Attack Time**: Once enemies detect a collision with the player, they take time to "swing"  (0.5s), and then after that time, if the player is still colliing with them, they deal damage.  
  
**Invincibility Frames**: For a short period after taking damage, the player is invulnerable, preventing the player getting "ping-ponged" around the screen by multiple enemies charging attacks at the same time.  
  
**Knockback**: The player AND enemies receive knockback when they are hurt. This clears out space for the player to move if surrounded, creating a more skill based, and less luck based game.

# Design Process: Tying it all together
Now we had the basic setup for enemy spawning, attacking, player movement and stats management, and player attacking. One thing that was majorly lacking was **win** and **lose** conditions. This was a simple fix, if the players health <= 0, we open up a "you lose" canvas
and force the player to either quit or restart. On the flipside, if all enemies are defeated, the player can walk to the right and transition to the next level.  
  
The game still felt empty though, it was devoid of life and functionality. In order to fix this, we added a main menu scene that sends the player to the character selector, which helped this feel like a real game, and less of a concept. This was really simple just
like the rest of the UI stuff. In our original wireframes and concepts, we planned to have an introductory cutscene that introduces the player to the main villain, Darren, as well as some basic backstory on where the zombies came from. To implement this, we copied the 
level one scene, removed the ability to move and zombie spawning, and then just manually moved the camera and characters through a script. We switched our scene order so that the character select screen comes after the cutscene, and our basic gameplay loop was completed.  

### Level Two
In order to add more depth to the game, we created a basic hallway map in photoshop and implemented a level two; a new and harder level with more zombies, more spawn locations for those zombies, and increased zombie health and damage. This was a simple tweak of
our enemy spawning script; nothing too complex. One problem I found here was that this map was a lot bigger, so we had to change the bounds we clamp the player to, as well as player speed, zombie speed, and zombie size. Once we fixed this, level two was fully implemented.  
  
### Audio
Kevin created all sound effects on his phone with his own voice. We then had to send these to our computers and convert them to WAV files because iPhone videos are m4a format, which Unity doesn't like at all. We added sound effects for getting hit and hitting zombies,
as well as zombies dying and when the player loses. This was essential for more feedback when you hit a zombie so you can clearly tell when you are dealing damage.

# Pitfalls
Some major pitfalls/sertbacks of this project were the art and animations. All of the animations (walk, run, punch) were created by Tyler in Aseprite, where I manually selected each leg, rotated it, and painted/fixed any areas that need it. This was very 
tedious and took a lot of time for just one animation. We decided that, for the scope of this game, it was important to at least include the animations for Greg as a "proof of concept". We wanted to show that we knew how to create these animations, without having to spend the time
of manually creating all of them for every character & the zombies.   

# Moving Forward
Given more time to produce this game, there are a few things we would want to implement:  
  
First and foremost, every character & zombie would have their own unique walking, running, and attacking animations, even possibly death animations. This would most likely be the most time
consuming improvement we could do.   
  
Next, we would absolutely add new levels with more spawn points and zombie types. It would be really neat to implement multiple archetypes of zombies; one zombie that is quick but weak, one that crawls but deals heavy damage, and a large
one that is slow and tanky but is relatively weak otherwise. This would add more variety to the game and make a more enjoyable experience for everybody around.   
  
Next, we would add more items and weapons that the player can use. Some immediate ideas that come to mind are a crowbar weapon which increases the players damage, pencils to throw at zombies for a lower damage but ranged attack, or slime-bombs that slow down
zombies wherever they land.   

Finally, if time allowed, we would have implemented a "party" system. Essentially, the player could choose a pool of characters such that they can switch between these characters when they wish. Due to the differing abilities between characters, this party would need
to be specially crafted to beat specific levels; for example maybe a level with tons of tanky enemies would be best beaten by Rowley, the character that does the most damage. The player would have to become familiar with the levels and decide which character they should 
use. 

# References
Kinney, Jeff. *Diary of a Wimpy Kid. Vol. 2: Rodrick Rules*. Amulet Books, 2008. 
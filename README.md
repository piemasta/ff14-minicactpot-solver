# MiniCactpot Solver
This is a solver/assistant program for the MiniCactpot lottery minigame in Final Fantasy 14. Input the initial number from a MiniCactpot card and the program will suggest a series of plays towards winning the Cactpot.

The goal of this program is to maximize a players winnings by providing play suggestions along with some basic probability imformation.

## Using the solver in-game
If you usually play with your game display set to Fullscreen, set it to Windowed Maximized so that MiniCactpot Solver will remain visible on top of your game.

You can change this by hitting Escape, ...

Upon entering the initial number, the solver will highlight one or more play spots with a light blue circle. While the user may then play any spot they like it is highly recommended to play highlighted spots, especially if the Cactpot Status indicator is still green. It's worth noting here that if you always follow the solver's suggestions, you will _always_ win the Cactpot if it is possible to do so.

After 4 plays have been entered (the initial number plus 3 user plays) the solver will suggest a pay line.

## Controls
While the mouse is hovering over a play spot:

  ***1-9**    Play number.

  ***0**      Reset the spot.

  ***Enter**  Clear all play spots.

  ***Escape** Exit the program.

## Special Features

##### Cactpot Status indicator
The Cactpot Status light in the bottom right of the solver window indicates whether or not it's still possible to win the Cactpot given the current plays. If the indicator is green, the solver will chase the Cactpot. If the light is red, the solver will instead try to direct you towards the play line with highest payout potential given the current set of plays.

##### Payout Chances (by line)
Hovering over a pay line arrow will cause the percent chances of winning each payout amount to display in the table on the right-hand side of the card.
  
## Notes

* The solver will silently prevent you from making illegal moves, or entering more than 4 total plays.

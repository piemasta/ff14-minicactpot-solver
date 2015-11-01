# MiniCactpot Solver

This is a solver/assistant program for the MiniCactpot lottery minigame in Final Fantasy 14. Input the initial number from a MiniCactpot card and the program will suggest a series of plays towards winning the Cactpot.

The goal of this program is to maximize a players winnings by providing play suggestions and some basic probability imformation.

## Using the solver in-game

If you usually play with your game display set to Fullscreen, set it to Windowed Maximized so that MiniCactpot Solver will remain visible on top of your game.

You can change this by hitting Escape, ...

Upon entering the initial number, the solver will highlight one or more play spots with a light blue circle. While the user may play any spot they like it is highly recommended to play highlighted spots, especially if the Cactpot Status indicator is still green. It's worth noting here that if you always follow the solver's suggestions, you will _always_ win the Cactpot if it is possible to do so.

After 4 plays have been entered (the initial number plus 3 user plays) the solver will suggest a pay line.

## Controls

The solver receives input via hovering the mouse over a play spot and typing a number **1-9**.

Use **0** to reset a play spot.

Press the **Enter** to reset all play spots.

Press **Escape** to exit the program.

## Special Features

##### Cactpot Status indicator
The Cactpot Status light in the bottom right of the solver window indicates whether or not it's still possible to win the Cactpot given the current plays. If the light is red, the solver will try to direct you towards the play line with highest payout potential.

##### Payout Chances (by line)
Hovering over a pay line arrow will cause the percent chances of winning each payout amount to display on the right-hand side of the card.
  
## Notes

* The solver will prevent you from making illegal moves, or entering more than 4 total plays.

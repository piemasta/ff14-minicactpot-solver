# MiniCactpot Solver
This is a solver/assistant program for the MiniCactpot lottery minigame in Final Fantasy 14. The goal of this program is to maximize a players winnings by providing a series of play suggestions along with some basic probability imformation.

## Using the solver in-game
By default, the solver is set to be an "Always on top" window. It will happily sit on top of the Final Fantasy 14 game client during use, as long as the client's screen mode is not set to fullscreen. You can change this by hitting Escape and opening System Configuration. The screen mode settings are in the Display Settings tab, and should be set to Borderless Windowed while using the solver. Windowed mode will work as well.

Upon entering the initial number, the solver will highlight one or more play spots with a light blue circle. While the user may then play any spot they like it is highly recommended to play highlighted spots, especially if the Cactpot Status indicator is still green.

After 4 plays have been entered (the initial number plus 3 user plays) the solver will suggest a pay line.

## Controls
While the mouse is hovering over a play spot:

  **1-9**    Play number.

  **0**      Reset the spot.

  **Enter**  Clear all play spots.

  **Escape** Exit the program.

## Special Features

##### Cactpot Status indicator
The Cactpot Status light in the bottom right of the solver window indicates whether or not it's still possible to win the Cactpot given the current plays. If the indicator is green, the solver will chase the Cactpot. If the light is red, the solver will instead try to direct you towards the play line with highest payout potential given the current set of plays.

##### Payout Chances (by line)
Hovering over a pay line arrow will cause the percent chances of winning each payout amount to display in the table on the right-hand side of the card.
  
## Cactpot Efficacy
The algorithm responsible for determining play suggestions when chasing the Cactpot does so by attempting to 'trap' it. In other words, it seeks to eliminate the Cactpot as a potential outcome on any play line as soon as possible. In every case, after 3 rounds of playing the solver's suggested play spots, there will only be *at most* a single play line that can still potentially be a Cactpot winner. What this means is that the solver will *always* lead the user to win the Cactpot payout, if it is possible to do so on the current ticket.

## Notes

* The solver will silently prevent you from making illegal moves, or entering more than 4 total plays.

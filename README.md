
Gene looked at the reports and sighed. How did it come to this... again???
Gathering everything together, he stomped into the main briefing room, looked around coldly at everyone, then slammed the pile of reports onto the table, sending papers flying all across the room.
"The budget projects for the next project are out of sight.... Again!!!"
"It's those darned SRBs, they're too big..." Bill replied meekly.
"They're still cheaper than making complete rockets" Bob continued even more softly...
Gene just glared...
"I have an idea," von Kerman raised his hand, and all eyes turned on him. "Why don't we make the SRBs the same way we make rockets; in parts"
Gene looked exasperated. "Because rockets use LIQUID FUEL!!!"
Werhner took a deep breath and smiled slyly....
"So?"
Gene rolled his eyes, "How do you intend to fuel the SRBs on the pad???  They ARE solid, you know!"
von Kerbman just smiled and continued unphased.
"Why don't me make standard sized modules, fill them with the solid propellant, and then stack them together. Just like we stack the liquid tanks"
Gene took a deep breath.  Everyone shrank back, expecting an explosion.  No explosion happened, other than Gene turning a bit greener.
"Ummm, explain yourself", Gene grumbled, then sighed in resignation. "Please".
"The thrust of an SRB is determined by how long it is." Von Kerman explained  "So, we stack segments to get the desired length... and Viola'"
Everyone watched amazed as Gene's eys crisscrossed a few times, then slowly focused back on von Kerman.
"You have a week to make it happen..." 


Features

SRB segments, 2, 4 and 8 meters long.  One part for each diameter using PartVariants
3 different engine styles, two parts for each engine, PartVariants used to scale engine to different sizes
Nosecones with built in parachutes and sepratrons, two parts, PartVariants used to scale engine to different sizes
Visual alarm in editor if SRB stack is too tall, outsized SRB stack gets highlighted

Abort mode, nosecones separate from stack, causing thrust be cancelled out by gases spewing from open stack
Failure modes if SRBs are made too long
Thrust directly relates to height, taller = more thrust
Two thrust models, one which matches stock thrust, one which matches BetterSRBs (about double stock)
Dev/test mode for testing, enables actions and PAW event to trigger engine failure
Audible alarm when engine failure occurs
SRB stack experiencing engine failure gets highlighted

Models have been made by @SuicidalInsanity
Consultative services provided by @OhioBob

Misc Notes
==========

See: https://en.wikipedia.org/wiki/Space_Shuttle_Solid_Rocket_Booster
	Internal configuration of fuel
		11 point star
		double-truncated cone

You probably already know this, but generally...

     Fuel mass is  proportional to   diameter ^ 2 * length

Likewise,

     Dry mass is  proportional to  diameter ^ 2 * length

Those relationships aren't perfect, but they are close.  The reason dry mass is proportional to diameter^2 and not just diameter is because the casing wall thickness increases proportional to the diameter.

I found that the Kickback was the most realistic in regard to its mass characteristics.  So for BetterSRBs I kept the Kickback's fuel and empty mass unchanged.  Everything else was sized in proportion to it.

=============================
https://www.rocketryforum.com


https://forum.kerbalspaceprogram.com/index.php?/topic/58236-15-real-fuels-v1273/&do=findComment&comment=3670577
Yeah, the CRP is very helpful here:

Tank def: Solid         Not in the CRP, this is described in a comment in the tank defs as "Legacy"

Tank Def:  HTPB       in the CRP describes it as HTPB, described in a comment in the tank defs as "Standard stuff"

Tank Def: PBAN        in the CRP it's described as PBAN, comment in the tank defs say "Shuttle SRB etc"
PBAN is a solid rocket fuel binder. Along with a curing agent, it is the "glue" that holds the solid chemicals together and gives the grains their shape
 

Same for HNIW, NGNC, and PSPC

HNIW comment in the tank defs says: "Speculative lower-smoke propellant (not in full production yet)"

NGNC comment in the tank defs says "SPRINT missile, etc"

PSPC comment in the tank defs says "Early propellant, pre-Aluminum. Think 'Tiny Tim'"


At least in the tank definitions there is a one line comment for each fuel type, but it's really not helpful at all. In my research on this, I found the following:

Double-base (DB) propellants: DB propellants are composed of two monopropellant fuel components where one typically acts as a high-energy (yet unstable) monopropellant and the other acts as a lower-energy stabilizing (and gelling) monopropellant.
Composite propellants: A powdered oxidizer and powdered metal fuel are intimately mixed and immobilized with a rubbery binder (that also acts as a fuel).
High-energy composite (HEC) propellants: Typical HEC propellants start with a standard composite propellant mixture (such as APCP) and add a high-energy explosive to the mix (not used that much)
Composite modified double base propellants: Composite modified double base propellants start with a nitrocellulose/nitroglycerin double base propellant as a binder and add solids (typically ammonium perchlorate (AP) and powdered aluminium) normally used in composite propellants
Minimum-signature (smokeless) propellants: One of the most active areas of solid propellant research is the development of high-energy, minimum-signature propellant using C6H6N6(NO2)6 CL-20 nitroamine (China Lake compound #20), which has 14% higher energy per mass and 20% higher energy density than HMX

and the most interesting one for me:

Electric solid propellants: Electric solid propellants (ESPs) are a family of high performance plastisol solid propellants that can be ignited and throttled by the application of electric current.

high performance electric propellant (HIPEP)  modern breed of electric solid propellants was developed with higher conductivity and specific impulse.



Current Issues
==============


need to disable thrust limiters and throttle in both engines and segments and activate engineAtl


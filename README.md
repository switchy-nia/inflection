# Aiko's Inflection (Unstable Pre-release/work in progress)

A plugin for Dalamud to modify speech input and output based on rules that you set for yourself.
This plugin is primarily intended for use _as a toy_ and can be used for RP, immersion or other.
**Disclaimer**: This is just a proof of concept that I made to see if I could. It's intended to be a toy, nothing more. Please treat it like that.

## Acknowledgements

Thank you to my darling Aiko for inspiring me to make this project, probably never would have even tried otherwise. <3

## Features

- Supports (Out of Character) and \*Custom Emote\* as per standard RP practice.
- Profiles (Including several builtin presets)
  - Builtin presets for soft voice, BIG VOICE, bimbo voice, catgirl nyaaa, meow voice, muted voice, and nervous voice
  - TODO: Import/Export to be able to share profiles with your other friends
- Text commands for automation and integration with puppetmaster,  character select+, etc.
- Speech alterations configurable with the following:
  - S-s-stuttering
  - Word substitution/matching
  - small voice setting so that you can yell in lower case...!
  - BIG TALK MODE FOR SERIOUS BIG VOICE PERSONS!!!!
  - Randomized Vocal Ticks (For adding a little bit of random flavour to boring sentences).
  - Sentence start/end "ticks"
  - Message Regular expression substitution
- TODO: Dalamud IPC functionality

## Stability (i.e. There is none)

TL;DR: This is currently a **very early work in progress** (consider it an in **development alpha build**).
Until v1.0 your configurations and profiles can and will be deleted if incompatible with the changes. (I'm sorry, I'd love to write a migration each time, but it's too much work for something that is prerelease)

Versioning follows [semantic versioning](www.semver.org) with the addition of the "build" (which will typically be 0 outside of testing releases).

### Testing branch

The testing branch plugins are inherently unstable. While you can grab this via the dalamud API it is not guaranteed to be stable and may delete your configuration.

Please do not use testing unless you have DMed me and are assisting me with testing this.

## Contributing

The project is not yet in a state to accept contributions or bug reports. If you have any issues or would like to make suggestions, please reach out to me on discord first.

## Download

Add the following repository to Dalamud to download

```sh
https://raw.githubusercontent.com/switchy-nia/inflection/main/repo.json
```

# Ambie
<img src="https://raw.githubusercontent.com/jenius-apps/ambie/main/images/logo_transparent.png" width="250">


A modern Windows app that plays soothing tones to help you be healthier, happier, and more relaxed.


Download from store: https://www.microsoft.com/store/productId/9P07XNM5CHP0

Chat about Ambie on Discord: https://discord.gg/b9z3BeXk3D

![](images/ambie_hero.png)

## Motivation

The motivation behind Ambie is to build a simple but beautiful soothing sounds player for Windows. Animations are used to enhance the experience. Only a few UI elements are available in the app. This is on purpose because for Ambie, _less is more_.

## Translation

Ambie needs volunteer translators! To help translate, follow these instructions.

### Adding a new language (requires Visual Studio 2019)
- Create a new issue with the subject `[Translation] fr-CA` where you replace `fr-CA` with whatever language-region code you'll be translating into.
    - If an issue already exists, then don't do this step.
- Fork and clone this repo
- Open in VS 2019
- In the `AmbientSounds.Uwp` project, find the `Strings` folder.
- Create a new folder inside `Strings` that looks like this: `en-US` but using the language you're translating into.
- Add a new `Resources.resw` item in that new folder
- Copy all the existing data from `Strings > en-US` into your new `Resources.resw`
- Translate the strings from english to your language
- Once done, then commit > push > create pull request!

### Improving an existing language (can be done with any text editor)
- Fork and clone this repo
- Open the the `.resw` file (e.g. `en-US > Resources.resw`) you want to edit. Choose any text editor
- Translate
- Commit > push > create pull request!

## Contributors

Pull requests are welcome! Please keep in mind the motivation behind Ambie, however: _Less is more_. New sound requests are appreciated. New sounds must
- Have a license that can work with Ambie
- Have an image
- Have attributions for sound and image

For all pull requests, please make sure there's an issue created for it first and that maintainers have confirmed that the issue or feature request can be addressed. Maintainers can then assign the issue to you so that others can track who's working on what issue. In some cases, you might have contacted the maintainers directly via other channels such as Twitter or Discord. You might have asked the maintainers if you can submit a PR. In those scenarios, an issue is not required.

## Attributions
- [Windows Community Toolkit](https://github.com/windows-toolkit/WindowsCommunityToolkit)
- [WinUI library](https://github.com/Microsoft/microsoft-ui-xaml)
- [Sounds](https://freesound.org). Specific file attributions are in `Data.json`.
- [Images](https://unsplash.com/). Specific image attributions are in `Data.json`.
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/)
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json/)
- [AppCenter Analytics](https://appcenter.ms/)

## Thank you translators!

- Korean: Jasmine
- Czech: [Michal Moudrý](https://github.com/MichalMoudry)
- Spanish: [Leonardo González Castro](https://github.com/OnlyOnePro), [Breece W](https://github.com/BreeceW)
- Hungarian: [Kristóf Kékesi](https://github.com/KristofKekesi)

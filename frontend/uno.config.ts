import { defineConfig, presetIcons, presetMini } from 'unocss'

export default defineConfig({
	shortcuts: {
		heading: 'text-4xl font-bold',
	},
	presets: [
		presetIcons({
			collections: {
				mdi: () => import('@iconify-json/mdi/icons.json').then((i) => i.default),
			},
		}),
		presetMini(),
	],
})

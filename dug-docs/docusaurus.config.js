module.exports = {
  title: 'dug',
  tagline: 'A global DNS propagation checker on your CLI!',
  url: 'https://dug.unfrl.com',
  baseUrl: '/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/logo.svg',
  organizationName: 'facebook', // Usually your GitHub org/user name.
  projectName: 'docusaurus', // Usually your repo name.
  themeConfig: {
    algolia: {
      apiKey: '1f056fb8cfb9b9676daf57f6bcc4766d',
      appId: "7G927L4XB6",
      indexName: 'unfrl',
      // contextualSearch: true,
      // Optional: Algolia search parameters
      // searchParameters: {},
    },
    colorMode: {
      defaultMode: 'dark'
    },
    navbar: {
      title: 'dug',
      logo: {
        alt: 'dug Logo',
        src: 'img/logo.svg',
        srcDark: 'img/logo_dark.svg',
      },
      items: [
        {
          to: 'docs/',
          activeBasePath: 'docs',
          label: 'Docs',
          position: 'left',
        },
        {
          href: 'https://github.com/unfrl/dug',
          label: 'Source Repository',
          position: 'left',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {
              label: 'Getting Started',
              to: 'docs/',
            },
            {
              label: 'Usage',
              to: 'docs/run',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'Unfrl (Hire us!)',
              href: 'https://unfrl.com',
            },
            {
              label: 'GitHub',
              href: 'https://github.com/unfrl/dug',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} <a href=https://unfrl.com>Unfrl LLC.</a>`,
    },
  },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl:
            'https://github.com/unfrl/dug/tree/main/dug-docs',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
};

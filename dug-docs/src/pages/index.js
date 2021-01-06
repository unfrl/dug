import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import useBaseUrl from '@docusaurus/useBaseUrl';
import styles from './styles.module.css';

const features = [
  {
    title: 'Blazing Fast',
    imageUrl: 'img/undraw_Outer_space_rocket.svg',
    description: (
      <>
        dug's querys are highly performant and as parallelizable as you want. Defaults to 200 simultaneous queries!
      </>
    ),
  },
  {
    title: 'Flexible Output',
    imageUrl: 'img/undraw_proud_coder.svg',
    description: (
      <>
        By default dug outputs pretty human-readable tables, but can easily be configured
        to provide templated JSON or CSV data. Use it in monitoring scripts or other applications!
      </>
    ),
  },
  {
    title: 'Highly Configurable',
    imageUrl: 'img/undraw_maintenance.svg',
    description: (
      <>
        Use the included servers or bring in your own list from local/remote sources. Control what
        queries are run and what servers they're run against.
      </>
    ),
  },
];

function Feature({imageUrl, title, description}) {
  const imgUrl = useBaseUrl(imageUrl);
  return (
    <div className={clsx('col col--4', styles.feature)}>
      {imgUrl && (
        <div className="text--center">
          <img className={styles.featureImage} src={imgUrl} alt={title} />
        </div>
      )}
      <h3>{title}</h3>
      <p>{description}</p>
    </div>
  );
}

function Home() {
  const context = useDocusaurusContext();
  const {siteConfig = {}} = context;
  return (
    <Layout
      title={`Know your DNS!`}
      description="A global DNS propagation checker">
      <header className={clsx('hero hero--primary', styles.heroBanner)}>
        <div className="container">
          <img src={useBaseUrl("img/logo.svg")} width="150px"/>
          <h1 className="hero__title">{siteConfig.title}</h1>
          <p className="hero__subtitle">{siteConfig.tagline}</p>
          <div className={styles.buttons}>
            <Link
              className={clsx(
                'button button--outline button--secondary button--lg',
                styles.getStarted,
              )}
              to={useBaseUrl('docs/')}>
              Get Started
            </Link>
          </div>
        </div>
      </header>
      <main>
        {features && features.length > 0 && (
          <section className={styles.features}>
            <div className="container">
              <div className="row">
                {features.map((props, idx) => (
                  <Feature key={idx} {...props} />
                ))}
              </div>
            </div>
          </section>
        )}
      </main>
    </Layout>
  );
}

export default Home;
